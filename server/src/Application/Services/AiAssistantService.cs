using System.Diagnostics;
using System.Text.Json;

using Application.AI.Contracts;

using Application.MCP;

namespace Application.AI;

public class AiAssistantService : IAiAssistantService
{
    // Tek bir kullanıcı mesajı için izin verilen en fazla LLM ↔ tool turu.
    private const int MaxIterations = 8;

    private readonly IAiProvider _aiProvider;
    private readonly IMcpClientService _mcpClientService;
    private readonly IToolContextResolver _toolContextResolver;

    public AiAssistantService(
        IAiProvider aiProvider,
        IMcpClientService mcpClientService,
        IToolContextResolver toolContextResolver)
    {
        _aiProvider = aiProvider;
        _mcpClientService = mcpClientService;
        _toolContextResolver = toolContextResolver;
    }

    public async Task<AiChatResponse> ChatAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        var tools =
            await _mcpClientService.GetToolDefinitionsAsync(
                cancellationToken);

        var messages = new List<ChatMessage>(
            request.Messages);

        var actions = new List<AiAction>();
        var trace = new List<AiTraceStep>();

        // Bu cevapta navigasyon üretildiyse kullanıcının varacağı sayfa (manifest page.id)
        string? navigatedPageId = null;

        for (var iteration = 1; iteration <= MaxIterations; iteration++)
        {
            var aiRequest = new AiRequest
            {
                CurrentPage = request.CurrentPage,
                Screen = request.Screen,
                Messages = messages,
                Tools = tools
            };

            var llmStopwatch = Stopwatch.StartNew();

            var response =
                await _aiProvider.ChatAsync(
                    aiRequest,
                    cancellationToken);

            trace.Add(new AiTraceStep
            {
                Kind = "llm",
                Iteration = iteration,
                Name = response.ToolCalls.Count == 0
                    ? "final_answer"
                    : $"{response.ToolCalls.Count} tool call",
                Result = response.Content,
                DurationMs = llmStopwatch.ElapsedMilliseconds
            });

            if (response.ToolCalls.Count == 0)
            {
                return new AiChatResponse
                {
                    Message = response.Content ?? string.Empty,
                    Actions = actions,
                    Trace = trace
                };
            }

            AddAssistantToolCallMessage(
                messages,
                response);

            foreach (var toolCall in response.ToolCalls)
            {
                var toolStopwatch = Stopwatch.StartNew();
                Dictionary<string, object?>? arguments = null;
                McpToolResult result;

                try
                {
                    arguments =
                        BuildToolArguments(
                            toolCall,
                            request.CurrentPage,
                            tools);

                    result =
                        await _mcpClientService.CallToolAsync(
                            toolCall.Name,
                            arguments,
                            cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Bozuk JSON argümanı, bilinmeyen tool vb. durumlarda
                    // hatayı modele geri ver; model toparlanmayı deneyebilsin.
                    result = new McpToolResult
                    {
                        Content = ex.Message,
                        IsError = true
                    };
                }

                AiAction? action = null;

                if (!result.IsError && result.StructuredContent.HasValue)
                {
                    action = TryCreateAction(result.StructuredContent.Value);

                    if (action != null && !IsUiAction(action))
                    {
                        action = null;
                    }

                    // Uygulanamayacak bir UI aksiyonunu istemciye göndermek yerine
                    // modele hata olarak döndür; kendini düzeltebilsin.
                    var rejection = action is null
                        ? null
                        : ValidateUiAction(action, navigatedPageId, request.Screen);

                    if (rejection != null)
                    {
                        action = null;
                        result = new McpToolResult
                        {
                            Content = rejection,
                            IsError = true
                        };
                    }
                }

                trace.Add(new AiTraceStep
                {
                    Kind = "tool",
                    Iteration = iteration,
                    Name = toolCall.Name,
                    Arguments = arguments is null
                        ? toolCall.Arguments
                        : JsonSerializer.Serialize(arguments),
                    Result = result.StructuredContent?.GetRawText()
                        ?? result.Content,
                    IsError = result.IsError,
                    DurationMs = toolStopwatch.ElapsedMilliseconds
                });

                AddToolResultMessage(
                    messages,
                    toolCall,
                    result);

                if (action != null)
                {
                    if (action.Type == AiActionTypes.Navigation)
                    {
                        navigatedPageId = GetDataString(action, "pageId");
                    }

                    actions.Add(action);
                }
            }
        }

        trace.Add(new AiTraceStep
        {
            Kind = "limit",
            Iteration = MaxIterations,
            Name = $"{MaxIterations} tur limiti aşıldı",
            IsError = true
        });

        return new AiChatResponse
        {
            Message = "İşlemi tamamlayamadım (tool çağrı limiti aşıldı). Lütfen isteğinizi daha net ifade edin.",
            Actions = actions,
            Trace = trace
        };
    }

    private Dictionary<string, object?> BuildToolArguments(
        AiToolCall toolCall,
        string? currentPage,
        List<AiToolDefinition> tools)
    {
        var arguments =
            JsonSerializer.Deserialize<
                Dictionary<string, object?>>(
                toolCall.Arguments)
            ?? new Dictionary<string, object?>();

        var tool =
            tools.FirstOrDefault(
                x => x.Name == toolCall.Name);

        if (tool == null)
        {
            return arguments;
        }

        // Model bazen alanları values sarmalayıcısı olmadan düz verir
        if (toolCall.Name == "fill_fields" && !arguments.ContainsKey("values"))
        {
            arguments = new Dictionary<string, object?> { ["values"] = arguments };
        }

        _toolContextResolver.ApplyContext(
            tool,
            arguments,
            currentPage);

        return arguments;
    }

    private static bool IsUiAction(
        AiAction action)
    {
        return action.Type switch
        {
            AiActionTypes.FillFields => true,
            AiActionTypes.Navigation => true,
            AiActionTypes.Notification => true,
            AiActionTypes.Highlight => true,
            _ => false
        };
    }

    /// <summary>
    /// İstemcide uygulanamayacağı baştan belli olan UI aksiyonlarını yakalar.
    /// İstemci aksiyonun sonucunu modele geri bildiremediği için (tek yönlü akış)
    /// bu kontrol olmadan model başarısız bir işlemi "yaptım" diye anlatır.
    /// Aksiyonun hedefi, kullanıcının aksiyonlar uygulandıktan sonra göreceği ekranda
    /// olmalı: navigasyon olduysa hedef sayfada (manifest), olmadıysa ekran özetinde.
    /// Sorun varsa modele gidecek hata mesajını döner.
    /// </summary>
    private static string? ValidateUiAction(
        AiAction action,
        string? navigatedPageId,
        ScreenSnapshot? screen)
    {
        return action.Type switch
        {
            AiActionTypes.FillFields => ValidateFillFields(action, navigatedPageId, screen),
            AiActionTypes.Highlight => ValidateHighlight(action, navigatedPageId, screen),
            _ => null
        };
    }

    private static string? ValidateFillFields(
        AiAction action,
        string? navigatedPageId,
        ScreenSnapshot? screen)
    {
        if (!action.Data.TryGetProperty("values", out var values) ||
            values.ValueKind != JsonValueKind.Object)
        {
            return "fill_fields için values boş.";
        }

        var keys = values.EnumerateObject().Select(item => item.Name).ToList();

        if (navigatedPageId != null)
        {
            // Hedef sayfa henüz ekranda değil; alanlarını Swagger şemasından (manifest) biliyoruz
            var fieldPages = action.Data.TryGetProperty("fieldPages", out var pages) ? pages : default;
            var unknown = keys
                .Where(key => !GetStrings(fieldPages, key).Contains(navigatedPageId))
                .ToList();

            return unknown.Count == 0
                ? null
                : $"'{navigatedPageId}' sayfasının formunda olmayan alan(lar): {string.Join(", ", unknown)}. " +
                  $"Alan adları için get_page_schema('{navigatedPageId}') kullan.";
        }

        var writable = screen?.Elements
            .Where(element => element.Kind is "input" or "textarea" or "select")
            .ToList() ?? [];

        if (writable.Count == 0)
        {
            return screen is null || screen.Elements.Count == 0
                ? null // ekran özeti yoksa doğrulanamaz; istemci dener
                : "Kullanıcının ekranında yazılabilir bir alan yok. Bir forma yazmak için önce o sayfaya git.";
        }

        var missing = keys.Where(key => !writable.Any(element => Matches(element, key))).ToList();

        return missing.Count == 0
            ? null
            : $"Ekranda olmayan alan(lar): {string.Join(", ", missing)}. Ekrandaki alanlar: " +
              string.Join(", ", writable.Select(Reference)) +
              ". Alan başka bir sayfadaysa önce navigate_to_page ile oraya git.";
    }

    private static string? ValidateHighlight(
        AiAction action,
        string? navigatedPageId,
        ScreenSnapshot? screen)
    {
        var elementId = GetDataString(action, "elementId") ?? string.Empty;
        var pageIds = GetStrings(action.Data, "pageIds");

        if (navigatedPageId != null)
        {
            return pageIds.Count == 0 || pageIds.Contains(navigatedPageId)
                ? null
                : $"'{elementId}' elemanı {string.Join(", ", pageIds)} sayfasında; kullanıcı navigasyondan sonra " +
                  $"'{navigatedPageId}' sayfasında olacak ve bu elemanı görmeyecek. " +
                  $"Hedef sayfadan bir eleman seç (get_page_schema '{navigatedPageId}').";
        }

        if (screen is null || screen.Elements.Count == 0 ||
            screen.Elements.Any(element => Matches(element, elementId)))
        {
            return null;
        }

        return $"'{elementId}' elemanı kullanıcının şu anki ekranında yok. Ekrandaki kimlikler: " +
               string.Join(", ", screen.Elements.Select(Reference)) +
               (pageIds.Count > 0
                   ? $". Bu eleman {string.Join(", ", pageIds)} sayfasında; önce navigate_to_page ile oraya git."
                   : ".");
    }

    // İstemci elemanı aynı sırayla bulur: data-ai-field → name → id
    private static bool Matches(ScreenElement element, string reference) =>
        element.Field == reference || element.Name == reference || element.Id == reference;

    private static string Reference(ScreenElement element) =>
        element.Field ?? element.Name ?? element.Id;

    private static List<string> GetStrings(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object &&
        element.TryGetProperty(propertyName, out var value) &&
        value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray().Select(item => item.GetString() ?? string.Empty).ToList()
            : [];

    private static string? GetDataString(AiAction action, string propertyName) =>
        action.Data.ValueKind == JsonValueKind.Object &&
        action.Data.TryGetProperty(propertyName, out var value) &&
        value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static AiAction? TryCreateAction(
        JsonElement structuredContent)
    {
        if (structuredContent.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!structuredContent.TryGetProperty(
                "type",
                out var typeProperty))
        {
            return null;
        }

        if (!structuredContent.TryGetProperty(
                "data",
                out var dataProperty))
        {
            return null;
        }

        var type = typeProperty.GetString();

        if (string.IsNullOrWhiteSpace(type))
        {
            return null;
        }

        JsonElement? target = null;

        if (structuredContent.TryGetProperty(
                "target",
                out var targetProperty))
        {
            target = targetProperty;
        }

        return new AiAction
        {
            Type = type,
            Target = target?.GetString(),
            Data = dataProperty
        };
    }

    private static void AddAssistantToolCallMessage(
        List<ChatMessage> messages,
        AiResponse response)
    {
        messages.Add(new ChatMessage
        {
            Role = "assistant",
            Content = response.Content,
            ToolCalls = response.ToolCalls
                .Select(toolCall => new AiToolCall
                {
                    Id = toolCall.Id,
                    Name = toolCall.Name,
                    Arguments = toolCall.Arguments
                })
                .ToList()
        });
    }

    private static void AddToolResultMessage(
        List<ChatMessage> messages,
        AiToolCall toolCall,
        McpToolResult result)
    {
        var content = result.IsError
            ? JsonSerializer.Serialize(new { error = result.Content })
            : result.StructuredContent.HasValue
                ? result.StructuredContent.Value.GetRawText()
                : result.Content;

        messages.Add(new ChatMessage
        {
            Role = "tool",
            ToolCallId = toolCall.Id,
            Content = content
        });
    }
}