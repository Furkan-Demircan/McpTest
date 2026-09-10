using Application.AI;
using Application.MCP;

using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Infrastructure.MCP;

public class McpClientService : IMcpClientService
{
    private readonly McpClient _client;
    private readonly IToolContextRegistry _toolContextRegistry;

    public McpClientService(McpClient client, IToolContextRegistry toolContextRegistry)
    {
        _client = client;
        _toolContextRegistry = toolContextRegistry;
    }

    public async Task<List<AiToolDefinition>> GetToolDefinitionsAsync(
        CancellationToken cancellationToken = default)
    {
        var tools = await _client.ListToolsAsync(
            cancellationToken: cancellationToken);

        return tools
    .Select(tool => new AiToolDefinition
    {
        Name = tool.Name,
        Description = tool.Description ?? string.Empty,
        Parameters = tool.JsonSchema,
        ContextType = _toolContextRegistry.GetContextType(tool.Name)
    })
    .ToList();
    }

    public async Task<McpToolResult> CallToolAsync(
        string toolName,
        IReadOnlyDictionary<string, object?>? arguments = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _client.CallToolAsync(
            toolName,
            arguments,
            cancellationToken: cancellationToken);

        var text = string.Join(
            "\n",
            result.Content
                .OfType<TextContentBlock>()
                .Select(content => content.Text));

        return new McpToolResult
        {
            Content = text,
            IsError = result.IsError ?? false
        };
    }
}