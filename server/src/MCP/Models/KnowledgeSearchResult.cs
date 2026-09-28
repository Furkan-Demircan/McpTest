namespace MCP.Server.Models
{
    public class KnowledgeSearchResult
    {
        public bool Found { get; set; }
        public string? Hint { get; set; }
        public List<KnowledgeGuide> Guides { get; set; } = [];
    }

    public class KnowledgeGuide
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<KnowledgeGuidePage> Pages { get; set; } = [];
        public double Score { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public class KnowledgeGuidePage
    {
        public string Id { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}
