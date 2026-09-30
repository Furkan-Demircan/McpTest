using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

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
    private readonly IPendingTurnStore _pendingTurnStore;

    public AiAssistantService(
        IAiProvider aiProvider,
        IMcpClientService mcpClientService,
        IToolContextResolver toolContextResolver,
        IPendingTurnStore pendingTurnStore)
    {
        _aiProvider = aiProvider;
        _mcpClientService = mcpClientService;
        _toolContextResolver = toolContextResolver;
        _pendingTurnStore = pendingTurnStore;
    }

    public Task<AiChatResponse> ChatAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        var turn = new PendingTurn
        {
            CurrentPage = request.CurrentPage,
            Messages = new List<ChatMessage>(request.Messages)
        };

        return RunLoopAsync(turn, request.Screen, screenAfterClientActions: false, cancellationToken);
    }

    public async Task<AiChatResponse?> ContinueAsync(
        AiContinueRequest request,
        CancellationToken cancellationToken = default)
    {
        var turn = _pendingTurnStore.Take(request.ContinuationId);
        if (turn is null)
        {
            return null;
        }

        // Bekleyen her UI tool çağrısının sonucu, istemcinin gerçekten uyguladığı sonuçtur.
        foreach (var pending in turn.Pending)
        {
            var result = request.Results.FirstOrDefault(item => item.ToolCallId == pending.ToolCallId)
                ?? new AiActionResult
                {
                    ToolCallId = pending.ToolCallId,
                    Status = "failed",
                    Error = "İstemci bu aksiyonun sonucunu bildirmedi."
                };

            var content = JsonSerializer.Serialize(new
            {
                clientResult = new
                {
                    status = result.Status,
                    detail = result.Detail,
                    error = result.Error
                }
            });

            turn.Messages.Add(new ChatMessage
            {
                Role = "tool",
                ToolCallId = pending.ToolCallId,
                Content = content
            });

            // Navigasyon gerçekten olduysa kullanıcı artık yeni sayfada
            if (pending.Action.Type == AiActionTypes.Navigation && result.Status == "applied")
            {
                turn.CurrentPage = GetDataString(pending.Action, "path") ?? turn.CurrentPage;
            }

            turn.Trace.Add(new AiTraceStep
            {
                Kind = "client",
                Iteration = turn.IterationsUsed,
                Name = $"{pending.ToolName} → {result.Status}",
                Result = content,
                IsError = result.Status != "applied"
            });
        }

        turn.Pending.Clear();

        return await RunLoopAsync(turn, request.Screen, screenAfterClientActions: true, cancellationToken);
    }

    /// <summary>
    /// LLM ↔ tool döngüsü. Sunucuda çalışan tool'ların (rehber, şema...) sonucu hemen modele
    /// yazılır. UI tool'larının sonucu ise istemci aksiyonu uygulayana kadar bekletilir:
    /// o LLM turunda UI aksiyonu varsa tur kaydedilir ve istemciye awaiting_client döner.
    /// </summary>
    private async Task<AiChatResponse> RunLoopAsync(
        PendingTurn turn,
        ScreenSnapshot? screen,
        bool screenAfterClientActions,
        CancellationToken cancellationToken)
    {
        var tools =
            await _mcpClientService.GetToolDefinitionsAsync(
                cancellationToken);

        // Bu adımda navigasyon üretildiyse kullanıcının varacağı sayfa (katalogdaki page.id).
        // Her devam adımı yeni ekran özetiyle başladığı için sıfırlanır.
        string? navigatedPageId = null;

        while (turn.IterationsUsed < MaxIterations)
        {
            var iteration = ++turn.IterationsUsed;

            var aiRequest = new AiRequest
            {
                CurrentPage = turn.CurrentPage,
                Screen = screen,
                ScreenAfterClientActions = screenAfterClientActions,
                Messages = turn.Messages,
                Tools = tools
            };

            var llmStopwatch = Stopwatch.StartNew();

            var response =
                await _aiProvider.ChatAsync(
                    aiRequest,
                    cancellationToken);

            turn.Trace.Add(new AiTraceStep
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
                // Tool sonuçları sohbet geçmişinde tutulmadığı için model kayıt bilgisini "hatırlayarak"
                // uydurabiliyor. Cevaptaki TC/e-posta hiçbir kaynakta yoksa bir kez tool ile doğrulat.
                // Ayrıca tool çağırmadan önceki bir cevabı aynen tekrarlıyorsa (örn. aynı liste sorusu) veri eskidir.
                var correction = FindUngroundedValues(turn.Messages, response.Content) is { Count: > 0 } ungrounded
                    ? ("kaynaksız kişisel veri", string.Join(", ", ungrounded),
                       $"Cevap taslağındaki şu değerler hiçbir tool sonucunda veya sohbette yok: {string.Join(", ", ungrounded)}. " +
                       "Kayıt bilgisini hafızandan yazma: ilgili tool'u (örn. find_student) şimdi çağır ve cevabı yalnızca onun sonucuna dayandır.")
                    : RepeatsPreviousAnswer(turn.Messages, response.Content)
                        ? ("önceki cevabın tekrarı", response.Content,
                           "Önceki bir cevabını aynen tekrarlıyorsun. Tool sonuçları geçmişte saklanmaz ve veriler değişmiş olabilir: " +
                           "istenen bilgiyi ilgili tool'u şimdi yeniden çağırarak al; tool gerekmiyorsa cevabını yeniden yaz.")
                        : default((string Name, string? Detail, string Instruction)?);

                if (correction is { } fix && !turn.AnswerRetried)
                {
                    turn.AnswerRetried = true;
                    turn.Trace.Add(new AiTraceStep
                    {
                        Kind = "limit",
                        Iteration = iteration,
                        Name = $"{fix.Name}, cevap yeniden istendi",
                        Result = fix.Detail,
                        IsError = true
                    });
                    turn.Messages.Add(new ChatMessage { Role = "system", Content = fix.Instruction });
                    continue;
                }

                return new AiChatResponse
                {
                    Status = AiChatStatus.Completed,
                    Message = response.Content ?? string.Empty,
                    Choices = turn.Choices,
                    Trace = TakeNewTrace(turn)
                };
            }

            AddAssistantToolCallMessage(
                turn.Messages,
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
                            turn.CurrentPage,
                            tools);

                    result = IsConfirmationWithoutUser(turn.Messages, toolCall)
                        ? new McpToolResult
                        {
                            Content = "Onay kullanıcıdan gelmeli: önizleme bu turda gösterildi. Önizlemeyi kullanıcıya " +
                                      "göster, 'Kaydedeyim mi?' diye sor ve cevabını bekle; confirmed=true'yu ancak " +
                                      "kullanıcı sonraki mesajında onaylarsa gönder.",
                            IsError = true
                        }
                        : await _mcpClientService.CallToolAsync(
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

                if (!result.IsError && result.StructuredContent.HasValue &&
                    TryReadChoices(result.StructuredContent.Value) is { } choices)
                {
                    turn.Choices = choices;
                }

                AiAction? action = null;

                if (!result.IsError && result.StructuredContent.HasValue)
                {
                    action = TryCreateAction(result.StructuredContent.Value);

                    if (action != null && !IsUiAction(action))
                    {
                        action = null;
                    }

                    // Uygulanamayacağı baştan belli bir UI aksiyonunu istemciye göndermek
                    // yerine modele hata olarak döndür; gidiş-dönüş olmadan düzeltebilsin.
                    var rejection = action is null
                        ? null
                        : ValidateUiAction(action, navigatedPageId, screen);

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

                turn.Trace.Add(new AiTraceStep
                {
                    Kind = "tool",
                    Iteration = iteration,
                    Name = action is null ? toolCall.Name : $"{toolCall.Name} (istemci sonucu bekleniyor)",
                    Arguments = arguments is null
                        ? toolCall.Arguments
                        : JsonSerializer.Serialize(arguments),
                    Result = result.StructuredContent?.GetRawText()
                        ?? result.Content,
                    IsError = result.IsError,
                    DurationMs = toolStopwatch.ElapsedMilliseconds
                });

                if (action is null)
                {
                    AddToolResultMessage(
                        turn.Messages,
                        toolCall,
                        result);
                    continue;
                }

                // UI aksiyonu: sonucu istemci uyguladıktan sonra ContinueAsync'te yazılır
                action.ToolCallId = toolCall.Id;
                turn.Pending.Add(new PendingToolCall(toolCall.Id, toolCall.Name, action));

                if (action.Type == AiActionTypes.Navigation)
                {
                    navigatedPageId = GetDataString(action, "pageId");
                }
            }

            if (turn.Pending.Count > 0)
            {
                var pendingActions = turn.Pending.Select(pending => pending.Action).ToList();
                var trace = TakeNewTrace(turn);

                return new AiChatResponse
                {
                    Status = AiChatStatus.AwaitingClient,
                    ContinuationId = _pendingTurnStore.Save(turn),
                    Actions = pendingActions,
                    Trace = trace
                };
            }
        }

        turn.Trace.Add(new AiTraceStep
        {
            Kind = "limit",
            Iteration = MaxIterations,
            Name = $"{MaxIterations} tur limiti aşıldı",
            IsError = true
        });

        return new AiChatResponse
        {
            Status = AiChatStatus.Completed,
            Message = "İşlemi tamamlayamadım (tool çağrı limiti aşıldı). Lütfen isteğinizi daha net ifade edin.",
            Trace = TakeNewTrace(turn)
        };
    }

    /// <summary>
    /// Onay gerektiren yazma tool'ları (örn. save_student) önce önizleme (needs_confirmation) döner,
    /// kullanıcı onaylayınca confirmed=true ile tekrar çağrılır. Onay kullanıcının SONRAKİ mesajından
    /// gelmelidir: son kullanıcı mesajından beri bu tool bir önizleme döndürdüyse, aynı turdaki
    /// confirmed=true çağrısı modelin kendi kararıdır ve reddedilir.
    /// </summary>
    private static bool IsConfirmationWithoutUser(
        List<ChatMessage> messages,
        AiToolCall toolCall)
    {
        if (!IsConfirmedCall(toolCall.Arguments))
        {
            return false;
        }

        var lastUserIndex = messages.FindLastIndex(message => message.Role == "user");

        var sameToolCallIds = messages
            .Skip(lastUserIndex + 1)
            .Where(message => message.ToolCalls != null)
            .SelectMany(message => message.ToolCalls!)
            .Where(call => call.Name == toolCall.Name)
            .Select(call => call.Id)
            .ToHashSet();

        return messages
            .Skip(lastUserIndex + 1)
            .Any(message =>
                message.Role == "tool" &&
                message.ToolCallId != null &&
                sameToolCallIds.Contains(message.ToolCallId) &&
                message.Content?.Contains("needs_confirmation", StringComparison.Ordinal) == true);
    }

    private static bool IsConfirmedCall(string? arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(arguments);
            return document.RootElement.ValueKind == JsonValueKind.Object &&
                   document.RootElement.TryGetProperty("confirmed", out var confirmed) &&
                   confirmed.ValueKind == JsonValueKind.True;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static readonly Regex PersonalValuePattern = new(
        @"(?<![\d])\d{11}(?![\d])|[\w.+-]+@[\w-]+(?:\.[\w-]+)+",
        RegexOptions.Compiled);

    /// <summary>
    /// Cevaptaki kişisel değerlerden (11 haneli TC, e-posta) sohbetin hiçbir mesajında
    /// (kullanıcı, önceki cevaplar, bu turun tool sonuçları) geçmeyenler.
    /// </summary>
    private static List<string> FindUngroundedValues(List<ChatMessage> messages, string? answer)
    {
        if (string.IsNullOrWhiteSpace(answer))
        {
            return [];
        }

        // Tool sonuçları JSON'dur; Türkçe karakterler \u kaçışlı olabilir, e-posta/TC için sorun değil
        var sources = string.Join("\n", messages.Select(message => message.Content)).ToLowerInvariant();

        return PersonalValuePattern.Matches(answer)
            .Select(match => match.Value.TrimEnd('.'))
            .Where(value => !sources.Contains(value.ToLowerInvariant(), StringComparison.Ordinal))
            .Distinct()
            .ToList();
    }

    // Bu istekte hiç tool çağrılmadan, geçmişteki bir asistan cevabıyla aynı metin mi üretildi
    private static bool RepeatsPreviousAnswer(List<ChatMessage> messages, string? answer)
    {
        var lastUserIndex = messages.FindLastIndex(message => message.Role == "user");
        if (string.IsNullOrWhiteSpace(answer) || messages.Skip(lastUserIndex + 1).Any(message => message.Role == "tool"))
        {
            return false;
        }

        var normalized = NormalizeAnswer(answer);
        return messages
            .Take(Math.Max(lastUserIndex, 0))
            .Any(message => message.Role == "assistant" &&
                            message.ToolCalls.Count == 0 &&
                            message.Content != null &&
                            NormalizeAnswer(message.Content) == normalized);
    }

    private static string NormalizeAnswer(string text) =>
        string.Concat(text.ToLowerInvariant().Where(char.IsLetterOrDigit));

    private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);

    // Tool sonucundaki "choices" dizisi (boş dizi listeyi temizler)
    private static List<AiChoice>? TryReadChoices(JsonElement structuredContent)
    {
        if (structuredContent.ValueKind != JsonValueKind.Object ||
            !structuredContent.TryGetProperty("choices", out var choices) ||
            choices.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        return choices.Deserialize<List<AiChoice>>(WebJson)?
            .Where(choice => !string.IsNullOrWhiteSpace(choice.Label) && !string.IsNullOrWhiteSpace(choice.Message))
            .ToList();
    }

    // Her cevapta istemciye sadece daha önce gönderilmemiş trace adımları gider
    private static List<AiTraceStep> TakeNewTrace(PendingTurn turn)
    {
        var steps = turn.Trace.Skip(turn.TraceSentCount).ToList();
        turn.TraceSentCount = turn.Trace.Count;
        return steps;
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
    /// olmalı: navigasyon olduysa hedef sayfada (katalog), olmadıysa ekran özetinde.
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
            // Hedef sayfa henüz ekranda değil; alanlarını Swagger şemasından (katalog) biliyoruz
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
            Data = dataProperty.Clone()
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