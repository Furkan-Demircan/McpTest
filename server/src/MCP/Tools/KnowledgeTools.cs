using System.ComponentModel;
using MCP.Server.Knowledge;
using MCP.Server.Manifest;
using MCP.Server.Models;
using ModelContextProtocol.Server;

namespace MCP.server.Tools;

[McpServerToolType]
public static class KnowledgeTools
{
    [McpServerTool(UseStructuredContent = true)]
    [Description(
    "Uygulamanın kullanım rehberlerinde arama yapar. " +
    "Kullanıcı bir işlemin NASIL yapılacağını sorduğunda, bir şeyi bilmediğini/bulamadığını söylediğinde, " +
    "bir hata aldığında veya uygulamanın bir özelliğini sorduğunda cevap vermeden ÖNCE MUTLAKA çağrılmalıdır. " +
    "Rehberler kullanım bilgisini (adım sırası, iş kuralları, bilinen kısıtlar, ekrandaki adlandırmalar) içerir; " +
    "ekran elemanları '\"Etiket\" butonu [id: ...]', form alanları '[alan: ...]' biçiminde geçer ve bu kimlikler " +
    "highlight_element / fill_fields ile kullanılabilir. Alanların tip ve kuralları (sözleşme) rehberde yoktur; " +
    "onlar için get_page_schema kullanılır.")]
    public static KnowledgeSearchResult SearchAppKnowledge(
        IKnowledgeRetriever retriever,
        AppManifestStore manifest,
        [Description("Kullanıcının sorusu veya aranan konu, örn: 'öğrenci nasıl eklenir', 'TC zaten mevcut hatası'.")]
        string query)
    {
        var matches = retriever.Search(query);

        return new KnowledgeSearchResult
        {
            Found = matches.Count > 0,
            Hint = matches.Count > 0
                ? null
                : "Bu konuda rehber bulunamadı. Bilgi uydurma; kullanıcıya bilmediğini söyle veya soruyu netleştirmesini iste.",
            Guides = matches
                .Select(match => new KnowledgeGuide
                {
                    Id = match.Document.Id,
                    Title = match.Document.Title,
                    Pages = match.Document.Pages
                        .Select(manifest.FindPage)
                        .OfType<PageDefinition>()
                        .Select(page => new KnowledgeGuidePage
                        {
                            Id = page.Id,
                            Path = page.Path,
                            Title = page.Title
                        })
                        .ToList(),
                    Score = match.Score,
                    Content = match.Document.Content
                })
                .ToList()
        };
    }
}
