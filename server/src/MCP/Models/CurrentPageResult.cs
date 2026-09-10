namespace MCP.Server.Models;

public class CurrentPageResult
{
    public string Type { get; set; } = "information";

    public string Page { get; set; } = string.Empty;

    public string PageName { get; set; } = string.Empty;
}