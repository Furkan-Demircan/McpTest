using Application.AI.Contracts;

namespace Application.AI;

public class ToolContextRegistry : IToolContextRegistry
{
    private static readonly Dictionary<string, AiToolContextType> Contexts =
        new()
        {
            ["get_form_status"] = AiToolContextType.Form
        };

    public AiToolContextType GetContextType(string toolName)
    {
        return Contexts.TryGetValue(
            toolName,
            out var contextType)
            ? contextType
            : AiToolContextType.None;
    }
}