using Application.AI.Contracts;

namespace Application.AI;

public class ToolContextRegistry : IToolContextRegistry
{
    private static readonly Dictionary<string, AiToolContextType> Contexts =
        new()
        {
            // Ekrandaki değerler her istekte ekran özetiyle modele gider;
            // tool argümanlarına sadece bulunulan sayfa enjekte edilir.
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