namespace Application.AI;

public interface IDeepSeekClient
{
    Task<AiChatResponse> ChatAsync(
        List<ChatMessage> message,
        Dictionary<string, string?> formData,
        CancellationToken cancellationToken = default);
}