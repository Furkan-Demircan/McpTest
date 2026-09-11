using System.Text.Json;
using System.Text.Json.Serialization;
namespace Application.AI.Contracts
{

    public class AiAction
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("target")]
        public string? Target { get; set; }

        [JsonPropertyName("data")]
        public JsonElement Data { get; set; }
    }
}