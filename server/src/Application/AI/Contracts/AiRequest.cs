namespace Application.AI;

public class AiRequest
{
    public string? CurrentPage { get; set; }

    public List<ChatMessage> Messages { get; set; } = [];

    public Dictionary<string, string?> FormData { get; set; } = [];

    public List<AiToolDefinition> Tools { get; set; } = [];
}