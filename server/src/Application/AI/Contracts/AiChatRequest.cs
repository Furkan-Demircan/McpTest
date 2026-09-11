namespace Application.AI;

public class AiChatRequest
{
    public string? CurrentPage { get; set; }
    public List<ChatMessage> Messages { get; set; } = [];

    public Dictionary<string, string?> FormData { get; set; } = [];
}