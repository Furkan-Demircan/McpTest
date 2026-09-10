using System.Text.Json;

namespace Application.MCP;

public class McpToolResult
{
    public string Content { get; set; } = string.Empty;

    public JsonElement? StructuredContent { get; set; }

    public bool IsError { get; set; }
}