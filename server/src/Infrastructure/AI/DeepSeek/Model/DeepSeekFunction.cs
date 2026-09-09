using System.Text.Json.Serialization;

namespace Infrastructure.AI.DeepSeek.Models;

public class DeepSeekFunction
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("arguments")]
    public string Arguments { get; set; } = string.Empty;
}