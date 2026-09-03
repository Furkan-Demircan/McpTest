namespace Application.AI;

public interface IDeepSeekClient
{
    Task<string> ChatAsync(
        string message,
        CancellationToken cancellationToken = default);
}