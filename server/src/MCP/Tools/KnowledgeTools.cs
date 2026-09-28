using System.ComponentModel;
using MCP.Server.Knowledge;
using MCP.Server.Manifest;
using MCP.Server.Models;
using ModelContextProtocol;
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
    "Rehberler süreç bilgisini (adım sırası, iş kuralları, bilinen kısıtlar) içerir; ekran elemanları " +
    "'\"Etiket\" butonu [id: ...]' biçiminde geçer ve bu kimlikler highlight_element ile kullanılabilir. " +
    "Alan listesi ve validasyon kuralları rehberde yoktur; onlar için get_page_schema kullanılır.")]
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

    [McpServerTool(UseStructuredContent = true)]
    [Description(
    "Ekrandaki bir alanı veya butonu kullanıcıya görsel olarak işaretler (kaydırıp vurgular, yanında kısa bir not gösterir). " +
    "Kullanıcıya bir adımı anlatırken 'nereye tıklayacağını / neyi dolduracağını' göstermek için kullanılır. " +
    "elementId uydurulmamalıdır: kullanıcının bulunduğu sayfa için ekran özetinden, başka bir sayfa için " +
    "get_page_schema veya search_app_knowledge sonucundan alınır. " +
    "Eleman başka bir sayfadaysa önce navigate_to_page çağrılmalıdır.")]
    public static HighlightResult HighlightElement(
        AppManifestStore manifest,
        [Description("İşaretlenecek elemanın kimliği, örn: 'firstName', 'studentSubmit', 'homeStudentLink'.")]
        string elementId,
        [Description("Elemanın yanında gösterilecek kısa yönlendirme notu, örn: 'Önce öğrencinin adını yazın'.")]
        string? message = null)
    {
        if (string.IsNullOrWhiteSpace(elementId))
        {
            throw new McpException("elementId boş olamaz.");
        }

        return new HighlightResult
        {
            Target = elementId,
            Data = new HighlightData
            {
                ElementId = elementId,
                Message = message,
                PageId = manifest.FindElement(elementId)?.PageId
            }
        };
    }
}
