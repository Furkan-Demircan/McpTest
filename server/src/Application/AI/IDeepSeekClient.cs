namespace Application.AI;

public interface IDeepSeekClient
{
    Task<string> ChatAsync(
        List<ChatMessage> message,
        CancellationToken cancellationToken = default);
}