using System.Text.RegularExpressions;
using MCP.Server.Manifest;

namespace MCP.Server.Knowledge;

/// <summary>
/// Rehberlerdeki [@referans] işaretlerini asistanın kullanabileceği kimliğe çevirir:
/// - Katalog elemanı: [@studentSubmit] → "Öğrenciyi Kaydet" butonu [id: studentSubmit]
/// - Form alanı (Swagger): [@firstName] → [alan: firstName]; alanın etiketini rehber
///   metni kendisi yazar, çünkü Swagger sözleşmedir, kullanım dili değil.
/// </summary>
public static partial class KnowledgeReferenceExpander
{
    [GeneratedRegex(@"\[@([\w-]+)\]")]
    private static partial Regex ReferencePattern();

    public static KnowledgeDocument Expand(KnowledgeDocument document, AppManifestStore manifest)
    {
        var content = ReferencePattern().Replace(document.Content, match =>
        {
            // Aynı alan adı birden fazla formda olabilir; rehberin sayfalarında ara
            var element = manifest.FindElement(match.Groups[1].Value, document.Pages);
            return element switch
            {
                null => match.Value,
                { Label: null } => $"[alan: {element.Id}]",
                _ => $"\"{element.Label}\" {KindLabel(element.Kind)} [id: {element.Id}]"
            };
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
