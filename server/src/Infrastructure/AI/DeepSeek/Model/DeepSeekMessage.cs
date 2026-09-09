using System.Text.Json.Serialization;

namespace Infrastructure.AI.DeepSeek.Models;

public class DeepSeekMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("tool_calls")]
    public List<DeepSeekToolCall> ToolCalls { get; set; } = [];
}