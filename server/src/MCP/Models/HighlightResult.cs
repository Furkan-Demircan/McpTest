namespace MCP.Server.Models
{
    public class HighlightResult
    {
        public string Type { get; set; } = "highlight";
        public string Target { get; set; } = string.Empty;
        public HighlightData Data { get; set; } = new();
    }

    public class HighlightData
    {
        public string ElementId { get; set; } = string.Empty;
        public string? Message { get; set; }

        // Elemanın manifest'te geçtiği sayfalar (aynı alan adı birden fazla formda olabilir)
        public List<string> PageIds { get; set; } = [];
    }
}
