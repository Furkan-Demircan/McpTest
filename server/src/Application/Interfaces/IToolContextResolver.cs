namespace Application.AI;

public interface IToolContextResolver
{
    void ApplyContext(
        AiToolDefinition toolDefinition,
        Dictionary<string, object?> arguments,
        Dictionary<string, string?> formData);
}