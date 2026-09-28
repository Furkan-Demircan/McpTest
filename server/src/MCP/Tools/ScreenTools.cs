using System.ComponentModel;
using MCP.Server.Manifest;
using MCP.Server.Models;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace MCP.server.Tools;

// Kullanıcının ekranında doğrudan etki eden aksiyonlar. Sunucu (AiAssistantService),
// hedef elemanın kullanıcının göreceği ekranda olup olmadığını ayrıca doğrular.
[McpServerToolType]
public static class ScreenTools
{
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
