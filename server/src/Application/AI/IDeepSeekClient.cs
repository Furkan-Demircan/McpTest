namespace Application.AI;

public interface IDeepSeekClient
{
    Task<AiChatResponse> ChatAsync(
        List<ChatMessage> message,
        CancellationToken cancellationToken = default);
}