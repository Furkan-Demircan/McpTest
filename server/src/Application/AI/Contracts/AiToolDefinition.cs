using Application.AI.Contracts;

namespace Application.AI;

public class AiToolDefinition
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public object Parameters { get; set; } = new();

    public AiToolContextType ContextType { get; set; }
}