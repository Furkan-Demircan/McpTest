using Application.AI.Contracts;

namespace Application.AI;

public class ToolContextResolver : IToolContextResolver
{
    public void ApplyContext(
        AiToolDefinition toolDefinition,
        Dictionary<string, object?> arguments,
        string? currentPage)
    {
        switch (toolDefinition.ContextType)
        {
            case AiToolContextType.Page:
                arguments["currentPage"] = currentPage;
                break;
            case AiToolContextType.None:
                break;
        }
    }
}
