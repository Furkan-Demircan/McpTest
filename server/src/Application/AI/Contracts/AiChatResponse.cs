using Application.AI.Contracts;

namespace Application.AI;

public class AiChatResponse
{
    // "completed": Message final cevaptır.
    // "awaiting_client": istemci Actions'ı uygulayıp sonuçları ContinuationId ile göndermeli.
    public string Status { get; set; } = AiChatStatus.Completed;

    public string? ContinuationId { get; set; }

    public string Message { get; set; } = string.Empty;
    public List<string> MissingFields { get; set; } = [];

    public List<AiAction> Actions { get; set; } = [];

    public List<AiTraceStep> Trace { get; set; } = [];
}

public static class AiChatStatus
{
    public const string Completed = "completed";
    public const string AwaitingClient = "awaiting_client";
}