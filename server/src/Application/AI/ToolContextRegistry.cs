using Application.AI.Contracts;

namespace Application.AI;

public class ToolContextRegistry : IToolContextRegistry
{
    private static readonly Dictionary<string, AiToolContextType> Contexts =
        new()
        {
            // Form verisi artık her istekte bağlam mesajıyla modele gidiyor;
            // tool argümanlarına ayrıca enjekte etmeye gerek yok.
            ["get_current_page"] = AiToolContextType.Page,
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