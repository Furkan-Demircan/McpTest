using System.Text.Json.Serialization;

namespace Infrastructure.AI.DeepSeek.Models;

public class DeepSeekToolCall
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("function")]
    public DeepSeekFunction Function { get; set; } = new();
}