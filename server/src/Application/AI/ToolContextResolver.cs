using Application.AI.Contracts;

namespace Application.AI;

public class ToolContextResolver : IToolContextResolver
{
    public void ApplyContext(
    AiToolDefinition toolDefinition,
    Dictionary<string, object?> arguments,
    Dictionary<string, string?> formData,
    string? currentPage)
    {
        switch (toolDefinition.ContextType)
        {
            case AiToolContextType.Form:
                ApplyFormContext(arguments, formData);
                break;

            case AiToolContextType.Page:
                arguments["currentPage"] = currentPage;
                break;
            case AiToolContextType.None:
                break;
        }
    }

    private static void ApplyFormContext(
    Dictionary<string, object?> arguments,
    Dictionary<string, string?> formData)
    {
        foreach (var field in formData)
        {
            if (!arguments.ContainsKey(field.Key))
            {
                arguments[field.Key] = field.Value;
            }
        }
    }
}