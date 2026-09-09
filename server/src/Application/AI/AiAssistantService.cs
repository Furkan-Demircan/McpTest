using System.Text.Json;

using Application.MCP;

namespace Application.AI;

public class AiAssistantService : IAiAssistantService
{
    private readonly IAiProvider _aiProvider;
    private readonly IMcpClientService _mcpClientService;

    public AiAssistantService(
        IAiProvider aiProvider,
        IMcpClientService mcpClientService)
    {
        _aiProvider = aiProvider;
        _mcpClientService = mcpClientService;
    }

    public async Task<AiChatResponse> ChatAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        var tools =
            await _mcpClientService.GetToolDefinitionsAsync(
                cancellationToken);

        var messages = new List<ChatMessage>(
            request.Messages);

        while (true)
        {
            var aiRequest = new AiRequest
            {
                Messages = messages,
                FormData = request.FormData,
                Tools = tools
            };

            var response =
                await _aiProvider.ChatAsync(
                    aiRequest,
                    cancellationToken);

            if (response.ToolCalls.Count == 0)
            {
                return new AiChatResponse
                {
                    Message = response.Content ?? string.Empty
                };
            }

            // DeepSeek'in yaptığı TÜM tool çağrılarını
            // tek bir assistant mesajı olarak ekliyoruz.
            var assistantMessage = new ChatMessage
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
            };

            messages.Add(assistantMessage);

            foreach (var toolCall in response.ToolCalls)
            {
                var argumentsDictionary =
                    JsonSerializer.Deserialize<
                        Dictionary<string, object?>>(
                        toolCall.Arguments)
                    ?? new Dictionary<string, object?>();

                // Form durumunu MCP tool'una aktar.
                if (toolCall.Name == "get_form_status")
                {
                    foreach (var formField in request.FormData)
                    {
                        argumentsDictionary[formField.Key] =
                            formField.Value;
                    }
                }

                var mcpResult =
                    await _mcpClientService.CallToolAsync(
                        toolCall.Name,
                        argumentsDictionary,
                        cancellationToken);

                Console.WriteLine(
                    $"MCP TOOL: {toolCall.Name}");

                Console.WriteLine(
                    $"MCP RESULT: {mcpResult.Content}");

                if (mcpResult.IsError)
                {
                    throw new InvalidOperationException(
                        $"MCP tool hatası: {mcpResult.Content}");
                }

                // MCP sonucunu conversation'a ekliyoruz.
                messages.Add(new ChatMessage
                {
                    Role = "tool",
                    ToolCallId = toolCall.Id,
                    Content = mcpResult.Content
                });

                // Form doldurma tool'u çalıştıysa
                // sonucu React'e formPatch olarak gönder.
                if (toolCall.Name == "fill_student_form")
                {
                    return CreateFormResponse(
                        mcpResult.Content,
                        request.FormData);
                }
            }
        }
    }

    private static AiChatResponse CreateFormResponse(
        string mcpContent,
        Dictionary<string, string?> currentFormData)
    {
        var formPatch =
            JsonSerializer.Deserialize<
                Dictionary<string, string?>>(
                mcpContent)
            ?? new Dictionary<string, string?>();

        return new AiChatResponse
        {
            Message = "Form bilgileri güncellendi.",
            FormPatch = formPatch,
            MissingFields = []
        };
    }
}