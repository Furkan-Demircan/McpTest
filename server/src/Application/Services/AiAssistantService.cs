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
                ActiveFormId = request.ActiveFormId,
                Screen = request.Screen,
                Messages = messages,
                FormData = request.FormData,
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
                            request.FormData,
                            request.CurrentPage,
                            request.ActiveFormId,
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
        Dictionary<string, string?> formData,
        string? currentPage,
        string? activeFormId,
        List<AiToolDefinition> tools
        )
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

        if (toolCall.Name == "fill_form")
        {
            if (!arguments.ContainsKey("values"))
            {
                var values = new Dictionary<string, object?>();
                string? target = null;

                foreach (var kvp in arguments)
                {
                    if (kvp.Key.Equals("target", StringComparison.OrdinalIgnoreCase))
                    {
                        target = kvp.Value?.ToString();
                    }
                    else
                    {
                        values[kvp.Key] = kvp.Value;
                    }
                }

                arguments.Clear();
                arguments["values"] = values;
                if (!string.IsNullOrWhiteSpace(target))
                {
                    arguments["target"] = target;
                }
            }

            // Hedef verilmediyse kullanıcının bulunduğu sayfanın formu (istemci manifest'ten çözer)
            if ((!arguments.ContainsKey("target") || arguments["target"] is null) &&
                !string.IsNullOrWhiteSpace(activeFormId))
            {
                arguments["target"] = activeFormId;
            }
        }

        _toolContextResolver.ApplyContext(
            tool,
            arguments,
            formData,
            currentPage);

        return arguments;
    }

    private static bool IsUiAction(
        AiAction action)
    {
        return action.Type switch
        {
            AiActionTypes.FormPatch => true,
            AiActionTypes.Navigation => true,
            AiActionTypes.Notification => true,
            AiActionTypes.Highlight => true,
            AiActionTypes.InputValue => true,
            _ => false
        };
    }

    /// <summary>
    /// İstemcide uygulanamayacağı baştan belli olan UI aksiyonlarını yakalar.
    /// İstemci aksiyonun sonucunu modele geri bildiremediği için (tek yönlü akış)
    /// bu kontrol olmadan model başarısız bir işlemi "yaptım" diye anlatır.
    /// - form_patch: hedef form çözülebilmeli (formsuz sayfada target şart)
    /// - highlight / input_value: eleman, kullanıcının aksiyonlar uygulandıktan sonra
    ///   göreceği ekranda olmalı; navigasyon olduysa hedef sayfada, olmadıysa ekran özetinde
    /// - input_value: form alanına değil, form dışı giriş alanına yazar
    /// Sorun varsa modele gidecek hata mesajını döner.
    /// </summary>
    private static string? ValidateUiAction(
        AiAction action,
        string? navigatedPageId,
        ScreenSnapshot? screen)
    {
        if (action.Type == AiActionTypes.FormPatch)
        {
            return string.IsNullOrWhiteSpace(action.Target)
                ? "Kullanıcının bulunduğu sayfada form yok ve hedef form (target) verilmedi; form doldurulmadı. " +
                  "Form dışı bir giriş alanına (arama kutusu, filtre) yazmak için set_input_value kullan; " +
                  "bir forma yazmak için o formun sayfasına git ve target ver."
                : null;
        }

        if (action.Type is not (AiActionTypes.Highlight or AiActionTypes.InputValue))
        {
            return null;
        }

        var elementId = GetDataString(action, "elementId");
        var elementPageId = GetDataString(action, "pageId");

        if (action.Type == AiActionTypes.InputValue)
        {
            // Manifest'te form alanı olarak tanımlıysa form durumu (FormContext) üzerinden yazılmalı
            if (GetDataString(action, "elementKind") == "field")
            {
                return $"'{elementId}' bir form alanı; set_input_value yerine fill_form kullan " +
                       "(values anahtarı alanın name değeridir, element id değil).";
            }

            var onScreen = screen?.Elements.FirstOrDefault(element => element.Id == elementId);
            if (navigatedPageId == null && onScreen != null &&
                onScreen.Kind is not ("input" or "textarea" or "select"))
            {
                return $"'{elementId}' bir giriş alanı değil ({onScreen.Kind}); değer yazılamaz.";
            }
        }

        if (navigatedPageId != null)
        {
            return elementPageId == null || elementPageId == navigatedPageId
                ? null
                : $"'{elementId}' elemanı '{elementPageId}' sayfasında; kullanıcı navigasyondan sonra " +
                  $"'{navigatedPageId}' sayfasında olacak ve bu elemanı görmeyecek. " +
                  $"Hedef sayfadan bir eleman seç (get_page_schema '{navigatedPageId}').";
        }

        if (screen is null || screen.Elements.Count == 0 ||
            screen.Elements.Any(element => element.Id == elementId))
        {
            return null;
        }

        return $"'{elementId}' elemanı kullanıcının şu anki ekranında yok. Ekrandaki kimlikler: " +
               string.Join(", ", screen.Elements.Select(element => element.Id)) +
               (elementPageId != null
                   ? $". Bu eleman '{elementPageId}' sayfasında; önce navigate_to_page ile oraya git."
                   : ".");
    }

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