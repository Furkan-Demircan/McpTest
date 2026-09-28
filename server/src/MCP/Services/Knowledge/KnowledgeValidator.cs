using System.Text.RegularExpressions;
using MCP.Server.Catalog;

namespace MCP.Server.Knowledge;

/// <summary>
/// Rehberlerin kataloğa verdiği referansları doğrular: `pages:` kimlikleri ve
/// [@elemanId] referansları. Katalog koddan ve Swagger'dan üretildiği için bir sayfa/alan
/// kaldırıldığında veya yeniden adlandırıldığında bayat kalan rehber açılışta görünür.
/// </summary>
public static partial class KnowledgeValidator
{
    [GeneratedRegex(@"\[@([\w-]+)\]")]
    private static partial Regex ReferencePattern();

    public static IReadOnlyList<string> Validate(
        IEnumerable<KnowledgeDocument> documents,
        AppCatalogStore catalog)
    {
        var issues = new List<string>();

        foreach (var document in documents)
        {
            foreach (var pageId in document.Pages.Where(pageId => catalog.FindPage(pageId) is null))
            {
                issues.Add($"Rehber '{document.Id}': bilinmeyen sayfa '{pageId}'");
            }

            foreach (Match match in ReferencePattern().Matches(document.Content))
            {
                if (catalog.FindElement(match.Groups[1].Value, document.Pages) is null)
                {
                    issues.Add($"Rehber '{document.Id}': [@{match.Groups[1].Value}] rehberin sayfalarında " +
                               $"({string.Join(", ", document.Pages)}) bulunamadı");
                }
            }
        }

        return issues;
    }
}
