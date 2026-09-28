using System.Text.RegularExpressions;
using MCP.Server.Manifest;

namespace MCP.Server.Knowledge;

/// <summary>
/// Rehberlerdeki [@elemanId] referanslarını manifest'teki güncel etikete çevirir:
/// [@studentSubmit] → "Öğrenciyi Kaydet" butonu [id: studentSubmit]
/// Etiket UI'da değişirse rehber elle güncellenmeden doğru kalır.
/// </summary>
public static partial class KnowledgeReferenceExpander
{
    [GeneratedRegex(@"\[@([\w-]+)\]")]
    private static partial Regex ReferencePattern();

    public static KnowledgeDocument Expand(KnowledgeDocument document, AppManifestStore manifest)
    {
        var content = ReferencePattern().Replace(document.Content, match =>
        {
            var element = manifest.FindElement(match.Groups[1].Value);
            return element is null
                ? match.Value
                : $"\"{element.Label}\" {KindLabel(element.Kind)} [id: {element.Id}]";
        });

        return new KnowledgeDocument
        {
            Id = document.Id,
            Title = document.Title,
            Pages = document.Pages,
            Keywords = document.Keywords,
            Content = content
        };
    }

    private static string KindLabel(string kind) => kind switch
    {
        "button" => "butonu",
        "link" => "bağlantısı",
        _ => "alanı"
    };
}
