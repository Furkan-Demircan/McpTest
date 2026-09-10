namespace Application.AI;

public interface IAiAssistantService
{
    Task<AiChatResponse> ChatAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default);
}