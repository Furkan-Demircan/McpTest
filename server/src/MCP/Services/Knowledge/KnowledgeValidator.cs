using System.Text.RegularExpressions;
using MCP.Server.Manifest;

namespace MCP.Server.Knowledge;

/// <summary>
/// Rehberlerin manifest'e verdiği referansları doğrular: `pages:` kimlikleri ve
/// [@elemanId] referansları. Manifest koddan üretildiği için bir sayfa/alan
/// kaldırıldığında veya yeniden adlandırıldığında bayat kalan rehber açılışta görünür.
/// </summary>
public static partial class KnowledgeValidator
{
    [GeneratedRegex(@"\[@([\w-]+)\]")]
    private static partial Regex ReferencePattern();

    public static IReadOnlyList<string> Validate(
        IEnumerable<KnowledgeDocument> documents,
        AppManifestStore manifest)
    {
        var issues = new List<string>();

        foreach (var document in documents)
        {
            foreach (var pageId in document.Pages.Where(pageId => manifest.FindPage(pageId) is null))
            {
                issues.Add($"Rehber '{document.Id}': bilinmeyen sayfa '{pageId}'");
            }

            foreach (Match match in ReferencePattern().Matches(document.Content))
            {
                if (manifest.FindElement(match.Groups[1].Value) is null)
                {
                    issues.Add($"Rehber '{document.Id}': bilinmeyen eleman [@{match.Groups[1].Value}]");
                }
            }
        }

        return issues;
    }
}
