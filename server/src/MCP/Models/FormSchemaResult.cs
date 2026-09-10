namespace MCP.Server.Models;

public class FormSchemaResult
{
    public string Type { get; set; } = "information";

    public string Form { get; set; } = "student";

    public List<FormField> Fields { get; set; } = [];
}

public class FormField
{
    public string Name { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}