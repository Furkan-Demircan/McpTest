namespace MCP.Server.Knowledge;

/// <summary>
/// Uygulama bilgi tabanındaki tek bir rehber (Knowledge/*.md).
/// RAG'e geçildiğinde bu doküman chunk'lanıp embedding'e dönüştürülecek birimdir.
/// </summary>
public class KnowledgeDocument
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    // Rehberin ilgili olduğu sayfa kimlikleri (manifest page.id)
    public IReadOnlyList<string> Pages { get; init; } = [];
    public IReadOnlyList<string> Keywords { get; init; } = [];
    public string Content { get; init; } = string.Empty;
}

public class KnowledgeMatch
{
    public KnowledgeDocument Document { get; init; } = default!;
    public double Score { get; init; }
}
