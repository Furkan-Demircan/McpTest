namespace Application.AI;

public class AiAssistantService : IAiAssistantService
{
    private readonly IDeepSeekClient _deepSeekClient;

    public AiAssistantService(
        IDeepSeekClient deepSeekClient)
    {
        _deepSeekClient = deepSeekClient;
    }

    public async Task<AiChatResponse> ChatAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _deepSeekClient.ChatAsync(
            request.Message,
            cancellationToken);

        return new AiChatResponse
        {
            Message = response
        };
    }
}
