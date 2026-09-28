using MCP.Server.Manifest;
using MCP.Server.Models;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCP.server.Tools;

// Sayfa bilgisinin tek kaynağı uygulama manifest'idir (AppManifestStore).
[McpServerToolType]
public static class ApplicationInfoTools
{
    [McpServerTool(UseStructuredContent = true)]
    [Description("Kullanıcının bulunduğu sayfanın kimliğini, adını ve varsa bağlı formunu döndürür.")]
    public static CurrentPageResult GetCurrentPage(
        AppManifestStore manifest,
        string? currentPage = null)
    {
        var page = manifest.FindPage(currentPage ?? "/");

        return new CurrentPageResult
        {
            Page = page?.Path ?? currentPage ?? "/",
            PageId = page?.Id,
            PageName = page?.Title ?? "Bilinmeyen Sayfa",
            FormId = page?.FormId
        };
    }

    [McpServerTool(UseStructuredContent = true)]
    [Description(
    "Uygulamadaki tüm sayfaları (kimlik, path, başlık, açıklama, bağlı form) listeler. " +
    "Hangi sayfaya gidileceğinden emin değilsen kullan.")]
    public static AppPagesResult ListAppPages(AppManifestStore manifest)
    {
        return new AppPagesResult
        {
            Pages = manifest.Manifest.Pages
                .Select(page => new AppPageSummary
                {
                    Id = page.Id,
                    Path = page.Path,
                    Title = page.Title,
                    Description = page.Description,
                    Module = page.Module,
                    FormId = page.FormId
                })
                .ToList()
        };
    }

    [McpServerTool(UseStructuredContent = true)]
    [Description(
    "Bir sayfanın yapısını döndürür: ekran elemanları (kimlik + etiket) ve sayfada form varsa " +
    "alanları, etiketleri, zorunlulukları, validasyon kuralları/mesajları ve ekran kimlikleri. " +
    "Kullanıcının BULUNMADIĞI bir sayfanın alanlarını anlatırken veya o sayfadaki bir elemanı " +
    "işaretlemeden önce kullan. Bulunduğu sayfanın elemanları zaten ekran özetinde gelir.")]
    public static PageSchemaResult GetPageSchema(
        AppManifestStore manifest,
        [Description("Sayfa kimliği (örn: 'student-create') veya path (örn: '/teacher').")]
        string page)
    {
        var definition = manifest.FindPage(page) ?? throw UnknownPage(manifest, page);

        return new PageSchemaResult
        {
            Page = definition,
            Form = manifest.FindForm(definition.FormId)
        };
    }

    [McpServerTool(UseStructuredContent = true)]
    [Description(
    "Kullanıcıyı uygulamadaki başka bir sayfaya yönlendirmek için navigation action üretir. " +
    "Sayfa kimliği (örn: 'student-create') veya path verilebilir; yalnızca manifest'teki sayfalar geçerlidir.")]
    public static NavigationResult NavigateToPage(
        AppManifestStore manifest,
        [Description("Hedef sayfa kimliği veya path'i.")]
        string page)
    {
        // Alias'lar da kabul edilir ama istemciye her zaman kanonik path gider.
        var definition = manifest.FindPage(page) ?? throw UnknownPage(manifest, page);

        return new NavigationResult
        {
            Type = "navigation",
            Data = new NavigationData
            {
                Path = definition.Path,
                PageId = definition.Id,
                FormId = definition.FormId
            }
        };
    }

    // McpException mesajı modele iletilir; model geçerli bir sayfa ile tekrar deneyebilir.
    private static McpException UnknownPage(AppManifestStore manifest, string page) =>
        new($"Geçersiz sayfa: '{page}'. Geçerli sayfalar: " +
            string.Join(", ", manifest.Manifest.Pages.Select(item => $"{item.Id} ({item.Path})")));
}
