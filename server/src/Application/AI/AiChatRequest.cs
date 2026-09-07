namespace Application.AI;

public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class AiChatRequest
{
    public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}