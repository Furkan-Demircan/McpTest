using Application.AI;

namespace Application.MCP;

public interface IMcpClientService
{
    Task<List<AiToolDefinition>> GetToolDefinitionsAsync(
        CancellationToken cancellationToken = default);

    Task<McpToolResult> CallToolAsync(
        string toolName,
        IReadOnlyDictionary<string, object?>? arguments = null,
        CancellationToken cancellationToken = default);
}