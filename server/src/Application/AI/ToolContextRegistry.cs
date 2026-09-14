using Application.AI.Contracts;

namespace Application.AI;

public class ToolContextRegistry : IToolContextRegistry
{
    private static readonly Dictionary<string, AiToolContextType> Contexts =
        new()
        {
            ["get_current_page"] = AiToolContextType.Page,
            ["get_form_status"] = AiToolContextType.Form,
            ["fill_student_form"] = AiToolContextType.Form,
            ["fill_teacher_form"] = AiToolContextType.Form
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