namespace MCP.Server.Knowledge;

/// <summary>
/// Basit anahtar kelime tabanlı retriever (eşleştirme: TurkishText).
/// Başlık/anahtar kelime eşleşmesi gövde eşleşmesinden daha ağırlıklıdır.
/// </summary>
public class KeywordKnowledgeRetriever : IKnowledgeRetriever
{
    private readonly IReadOnlyList<IndexedDocument> _documents;

    public KeywordKnowledgeRetriever(IEnumerable<KnowledgeDocument> documents)
    {
        _documents = documents
            .Select(document => new IndexedDocument(
                document,
                TurkishText.Tokenize($"{document.Title} {string.Join(' ', document.Keywords)}"),
                TurkishText.Tokenize(document.Content)))
            .ToList();
    }

    public IReadOnlyList<KnowledgeMatch> Search(string query, int maxResults = 2)
    {
        var queryTokens = TurkishText.QueryTokens(query);

        if (queryTokens.Count == 0)
        {
            return [];
        }

        return _documents
            .Select(document => new KnowledgeMatch
            {
                Document = document.Source,
                Score = queryTokens.Sum(token =>
                    (document.HeadTokens.Any(head => TurkishText.IsMatch(token, head)) ? 3 : 0) +
                    (document.BodyTokens.Any(body => TurkishText.IsMatch(token, body)) ? 1 : 0))
            })
            .Where(match => match.Score > 0)
            .OrderByDescending(match => match.Score)
            .Take(maxResults)
            .ToList();
    }

    private sealed record IndexedDocument(
        KnowledgeDocument Source,
        HashSet<string> HeadTokens,
        HashSet<string> BodyTokens);
}
