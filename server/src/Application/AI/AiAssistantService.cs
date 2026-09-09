using System.Text.Json;
using Application.AI.Tools;

namespace Application.AI;

public class AiAssistantService : IAiAssistantService
{
    private readonly IAiProvider _aiProvider;
    private readonly FillStudentFormHandler _fillStudentFormHandler;

    public AiAssistantService(
        IAiProvider aiProvider,
        FillStudentFormHandler fillStudentFormHandler)
    {
        _aiProvider = aiProvider;
        _fillStudentFormHandler = fillStudentFormHandler;
    }

    public async Task<AiChatResponse> ChatAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        var aiRequest = new AiRequest
        {
            Messages = request.Messages,
            FormData = request.FormData
        };

        var response = await _aiProvider.ChatAsync(
            aiRequest,
            cancellationToken);

        if (response.ToolCalls.Count == 0)
        {
            return new AiChatResponse
            {
                Message = response.Content ?? string.Empty
            };
        }

        foreach (var toolCall in response.ToolCalls)
        {
            if (toolCall.Name == "fill_student_form")
            {
                var arguments =
                    JsonSerializer.Deserialize<FillStudentFormArguments>(
                        toolCall.Arguments,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (arguments is null)
                {
                    throw new InvalidOperationException(
                        "fill_student_form arguments deserialize edilemedi.");
                }

                return _fillStudentFormHandler.Handle(
                    arguments,
                    request.FormData);
            }
        }

        throw new InvalidOperationException(
            "Desteklenmeyen bir AI tool çağrısı alındı.");
    }
}