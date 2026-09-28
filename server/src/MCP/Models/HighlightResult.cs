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

        // Eleman manifest'te tanımlıysa bulunduğu sayfa (bilgi amaçlı)
        public string? PageId { get; set; }
    }
}
