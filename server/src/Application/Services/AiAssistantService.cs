using System.Text.Json;
using Application.AI.Contracts;
using Application.Mcp;
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

        var actions = new List<AiAction>();

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
                    Message = response.Content ?? string.Empty,
                    Actions = actions
                };
            }

            AddAssistantToolCallMessage(
                messages,
                response);

            foreach (var toolCall in response.ToolCalls)
            {
                var arguments =
                    BuildToolArguments(
                        toolCall,
                        request.FormData,
                        tools);

                var result =
                    await _mcpClientService.CallToolAsync(
                        toolCall.Name,
                        arguments,
                        cancellationToken);

                if (result.IsError)
                {
                    throw new InvalidOperationException(
                        $"MCP tool hatası: {result.Content}");
                }

                AddToolResultMessage(
                    messages,
                    toolCall,
                    result);

                var action = CreateAction(result.Content);

                if (IsUiAction(action))
                {
                    actions.Add(action);
                }
            }
        }
    }

    private static bool IsUiAction(AiAction action)
    {
        return action.Type switch
        {
            AiActionTypes.FormPatch => true,
            AiActionTypes.Navigation => true,
            AiActionTypes.Notification => true,
            _ => false
        };
    }

    private static void AddAssistantToolCallMessage(
        List<ChatMessage> messages,
        AiResponse response)
    {
        messages.Add(new ChatMessage
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
        });
    }

    private static void AddToolResultMessage(
        List<ChatMessage> messages,
        AiToolCall toolCall,
        McpToolResult result)
    {
        messages.Add(new ChatMessage
        {
            Role = "tool",
            ToolCallId = toolCall.Id,
            Content = result.Content
        });
    }

    private static AiAction CreateAction(string mcpContent)
    {
        McpActionResult? result;

        try
        {
            result =
                JsonSerializer.Deserialize<McpActionResult>(
                    mcpContent);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                "MCP sonucu geçerli bir action JSON değil.",
                ex);
        }

        if (result == null)
        {
            throw new InvalidOperationException(
                "MCP sonucu boş.");
        }

        if (string.IsNullOrWhiteSpace(result.Type))
        {
            throw new InvalidOperationException(
                "MCP action type bilgisi bulunamadı.");
        }

        if (result.Data.ValueKind == JsonValueKind.Undefined)
        {
            throw new InvalidOperationException(
                $"MCP action '{result.Type}' için data bilgisi bulunamadı.");
        }

        return new AiAction
        {
            Type = result.Type,
            Data = result.Data
        };
    }

    private static Dictionary<string, object?> BuildToolArguments(
        AiToolCall toolCall,
        Dictionary<string, string?> formData,
        List<AiToolDefinition> tools)
    {
        var arguments =
            JsonSerializer.Deserialize<
                Dictionary<string, object?>>(
                toolCall.Arguments)
            ?? new Dictionary<string, object?>();

        var tool = tools.FirstOrDefault(
            x => x.Name == toolCall.Name);

        if (tool == null)
            return arguments;

        var parametersJson =
            JsonSerializer.SerializeToElement(
                tool.Parameters);

        if (!parametersJson.TryGetProperty(
                "properties",
                out var properties))
        {
            return arguments;
        }

        foreach (var property in properties.EnumerateObject())
        {
            var fieldName = property.Name;

            if (formData.TryGetValue(
                    fieldName,
                    out var value))
            {
                arguments[fieldName] = value;
            }
        }

        return arguments;
    }
}