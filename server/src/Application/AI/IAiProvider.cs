namespace Application.AI;

public interface IAiProvider
{
    Task<AiResponse> ChatAsync(
        AiRequest request,
        CancellationToken cancellationToken = default);
}