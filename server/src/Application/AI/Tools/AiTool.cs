namespace Application.AI.Tools;

public class AiTool
{
    public string Type { get; set; } = "function";

    public AiFunction Function { get; set; } = new();
}

public class AiFunction
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public object Parameters { get; set; } = new();
}