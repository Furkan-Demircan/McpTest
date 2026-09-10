using Application.AI.Contracts;

namespace Application.AI;

public class ToolContextResolver : IToolContextResolver
{
    public void ApplyContext(
        AiToolDefinition toolDefinition,
        Dictionary<string, object?> arguments,
        Dictionary<string, string?> formData)
    {
        switch (toolDefinition.ContextType)
        {
            case AiToolContextType.Form:
                ApplyFormContext(arguments, formData);
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
            arguments[field.Key] = field.Value;
        }
    }
}