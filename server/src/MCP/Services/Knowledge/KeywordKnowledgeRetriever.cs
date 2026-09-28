using System.Globalization;
using System.Text;

namespace MCP.Server.Knowledge;

/// <summary>
/// Basit anahtar kelime tabanlı retriever. Türkçe ekler için kelimeleri
/// ortak önek üzerinden eşler ("öğrenciyi" ~ "öğrenci", "eklerim" ~ "ekle").
/// Başlık/anahtar kelime eşleşmesi gövde eşleşmesinden daha ağırlıklıdır.
/// </summary>
public class KeywordKnowledgeRetriever : IKnowledgeRetriever
{
    private const int MinPrefixLength = 4;

    private static readonly HashSet<string> StopWords =
    [
        "ben", "sen", "bu", "su", "bir", "ve", "ile", "icin", "ama", "nasil",
        "ne", "neden", "nerede", "mi", "mu", "misin", "musun", "bilmiyorum",
        "yapabilirim", "yapilir", "olur", "olacak", "olacagini", "istiyorum",
        "lazim", "gerek", "yardim", "sistemde", "sisteme", "yeni", "bana", "beni"
    ];

    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    private readonly IReadOnlyList<IndexedDocument> _documents;

    public KeywordKnowledgeRetriever(IEnumerable<KnowledgeDocument> documents)
    {
        _documents = documents
            .Select(document => new IndexedDocument(
                document,
                Tokenize($"{document.Title} {string.Join(' ', document.Keywords)}"),
                Tokenize(document.Content)))
            .ToList();
    }

    public IReadOnlyList<KnowledgeMatch> Search(string query, int maxResults = 2)
    {
        var queryTokens = Tokenize(query)
            .Where(token => !StopWords.Contains(token))
            .Distinct()
            .ToList();

        if (queryTokens.Count == 0)
        {
            return [];
        }

        return _documents
            .Select(document => new KnowledgeMatch
            {
                Document = document.Source,
                Score = queryTokens.Sum(token =>
                    (document.HeadTokens.Any(head => IsMatch(token, head)) ? 3 : 0) +
                    (document.BodyTokens.Any(body => IsMatch(token, body)) ? 1 : 0))
            })
            .Where(match => match.Score > 0)
            .OrderByDescending(match => match.Score)
            .Take(maxResults)
            .ToList();
    }

    private static bool IsMatch(string queryToken, string documentToken)
    {
        if (queryToken == documentToken)
        {
            return true;
        }

        var prefixLength = Math.Min(queryToken.Length, documentToken.Length);
        return prefixLength >= MinPrefixLength &&
               string.CompareOrdinal(queryToken, 0, documentToken, 0, prefixLength) == 0;
    }

    private static HashSet<string> Tokenize(string text)
    {
        var normalized = new StringBuilder(text.Length);

        foreach (var ch in text.ToLower(Turkish))
        {
            normalized.Append(ch switch
            {
                'ç' => 'c',
                'ğ' => 'g',
                'ı' => 'i',
                'ö' => 'o',
                'ş' => 's',
                'ü' => 'u',
                _ => char.IsLetterOrDigit(ch) ? ch : ' '
            });
        }

        return normalized
            .ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(token => token.Length > 1)
            .ToHashSet();
    }

    private sealed record IndexedDocument(
        KnowledgeDocument Source,
        HashSet<string> HeadTokens,
        HashSet<string> BodyTokens);
}
