namespace Application.AI;


public class AiResponse
{
    public string? Content { get; set; }

    public List<AiToolCall> ToolCalls { get; set; } = [];
}