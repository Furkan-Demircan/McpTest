using Application.AI.Contracts;

namespace Application.AI;

public class AiRequest
{
    public string? CurrentPage { get; set; }

    public ScreenSnapshot? Screen { get; set; }

    public List<ChatMessage> Messages { get; set; } = [];

    public List<AiToolDefinition> Tools { get; set; } = [];
}
