namespace Application.AI;

public class ChatMessage
{
    public string Role { get; set; } = string.Empty;

    public string? Content { get; set; }

    public List<AiToolCall> ToolCalls { get; set; } = [];

    public string? ToolCallId { get; set; }
}