using System.Text.Json;

using Application.AI.Contracts;

using Application.Mcp;

using Application.MCP;

namespace Application.AI;

public class AiAssistantService : IAiAssistantService
{
    private readonly IAiProvider _aiProvider;
    private readonly IMcpClientService _mcpClientService;
    private readonly IToolContextResolver _toolContextResolver;

    public AiAssistantService(
        IAiProvider aiProvider,
        IMcpClientService mcpClientService,
        IToolContextResolver toolContextResolver)
    {
        _aiProvider = aiProvider;
        _mcpClientService = mcpClientService;
        _toolContextResolver = toolContextResolver;
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

                if (result.StructuredContent.HasValue)
                {
                    var action =
                        TryCreateAction(
                            result.StructuredContent.Value);

                    if (action != null &&
                        IsUiAction(action))
                    {
                        actions.Add(action);
                    }
                }
            }
        }
    }

    private Dictionary<string, object?> BuildToolArguments(
        AiToolCall toolCall,
        Dictionary<string, string?> formData,
        List<AiToolDefinition> tools)
    {
        var arguments =
            JsonSerializer.Deserialize<
                Dictionary<string, object?>>(
                toolCall.Arguments)
            ?? new Dictionary<string, object?>();

        var tool =
            tools.FirstOrDefault(
                x => x.Name == toolCall.Name);

        if (tool == null)
        {
            return arguments;
        }

        _toolContextResolver.ApplyContext(
            tool,
            arguments,
            formData);

        return arguments;
    }

    private static bool IsUiAction(
        AiAction action)
    {
        return action.Type switch
        {
            AiActionTypes.FormPatch => true,
            AiActionTypes.Navigation => true,
            AiActionTypes.Notification => true,
            _ => false
        };
    }

    private static AiAction? TryCreateAction(
        JsonElement structuredContent)
    {
        if (structuredContent.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!structuredContent.TryGetProperty(
                "type",
                out var typeProperty))
        {
            return null;
        }

        if (!structuredContent.TryGetProperty(
                "data",
                out var dataProperty))
        {
            return null;
        }

        var type = typeProperty.GetString();

        if (string.IsNullOrWhiteSpace(type))
        {
            return null;
        }

        JsonElement? target = null;

        if (structuredContent.TryGetProperty(
                "target",
                out var targetProperty))
        {
            target = targetProperty;
        }

        return new AiAction
        {
            Type = type,
            Target = target?.GetString(),
            Data = dataProperty
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
        var content = result.StructuredContent.HasValue
            ? result.StructuredContent.Value.GetRawText()
            : result.Content;

        messages.Add(new ChatMessage
        {
            Role = "tool",
            ToolCallId = toolCall.Id,
            Content = content
        });
    }
}