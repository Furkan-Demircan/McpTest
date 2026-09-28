namespace MCP.Server.Models
{
    public class FillFieldsResult
    {
        public string Type { get; set; } = "fill_fields";
        public FillFieldsData Data { get; set; } = new();
    }

    public class FillFieldsData
    {
        // Alan referansı (data-ai-field / name / id) → yazılacak değer
        public Dictionary<string, object?> Values { get; set; } = [];

        // Her alanın katalogda (Swagger şemalarında) geçtiği sayfalar; sunucu doğrulaması için
        public Dictionary<string, List<string>> FieldPages { get; set; } = [];
    }
}
