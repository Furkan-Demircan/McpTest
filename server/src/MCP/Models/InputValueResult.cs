namespace MCP.Server.Models
{
    public class InputValueResult
    {
        public string Type { get; set; } = "input_value";
        public string Target { get; set; } = string.Empty;
        public InputValueData Data { get; set; } = new();
    }

    public class InputValueData
    {
        public string ElementId { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        // Eleman manifest'te tanımlıysa bulunduğu sayfa ve türü; sunucu doğrulaması için
        public string? PageId { get; set; }
        public string? ElementKind { get; set; }
    }
}
