namespace MCP.Server.Models
{
    public class NavigationResult
    {
        public string Type { get; set; } = "navigation(";
        public NavigationData Data { get; set; } = new();
    }

    public class NavigationData
    {
        public string Path { get; set; } = string.Empty;
    }
}