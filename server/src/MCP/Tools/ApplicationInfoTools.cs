using MCP.Server.Catalog;
using MCP.Server.Models;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCP.server.Tools;

// Sayfa bilgisinin tek kaynağı uygulama kataloğudir (AppCatalogStore).
[McpServerToolType]
public static class ApplicationInfoTools
{
    [McpServerTool(UseStructuredContent = true)]
    [Description("Kullanıcının bulunduğu sayfanın kimliğini, adını ve varsa bağlı formunu döndürür.")]
    public static CurrentPageResult GetCurrentPage(
        AppCatalogStore catalog,
        string? currentPage = null)
    {
        var page = catalog.FindPage(currentPage ?? "/");

        return new CurrentPageResult
        {
            Page = page?.Path ?? currentPage ?? "/",
            PageId = page?.Id,
            PageName = page?.Title ?? "Bilinmeyen Sayfa",
            FormId = page?.FormId
        };
    }

    // Büyük uygulamada (yüzlerce sayfa) tool cevabını küçük tutmak için üst sınır
    private const int MaxPageResults = 10;

    [McpServerTool(UseStructuredContent = true)]
    [Description(
    "Uygulamadaki sayfaları arar. Hangi sayfaya gidileceğini veya bir işlemin hangi sayfada " +
    "yapıldığını bilmiyorsan kullan. 'query' ile konuya göre en ilgili sayfaları, 'module' ile bir " +
    "modülün sayfalarını döner. Parametresiz çağrı modül listesini döner (sayfa sayısı azsa sayfaları da).")]
    public static AppPagesResult ListAppPages(
        AppCatalogStore catalog,
        [Description("Aranan konu, örn: 'öğretmen ekleme', 'kayıt listesi'.")]
        string? query = null,
        [Description("Modül adı, örn: 'Okul'. Modül listesi için parametresiz çağır.")]
        string? module = null)
    {
        var total = catalog.Catalog.Pages.Count;
        var browseAll = string.IsNullOrWhiteSpace(query) && string.IsNullOrWhiteSpace(module);

        var pages = browseAll && total > MaxPageResults
            ? []
            : catalog.SearchPages(query, module, MaxPageResults);

        return new AppPagesResult
        {
            TotalPages = total,
            Modules = catalog.Modules()
                .Select(item => new AppModuleSummary { Name = item.Module, PageCount = item.PageCount })
                .ToList(),
            Pages = pages
                .Select(page => new AppPageSummary
                {
                    Id = page.Id,
                    Path = page.Path,
                    Title = page.Title,
                    Description = page.Description,
                    Module = page.Module,
                    FormId = page.FormId
                })
                .ToList(),
            Hint = pages.Count == 0
                ? browseAll
                    ? "Sayfa çok; 'query' veya 'module' ile ara."
                    : "Eşleşen sayfa yok; farklı kelimelerle veya modül adıyla dene."
                : null
        };
    }

    [McpServerTool(UseStructuredContent = true)]
    [Description(
    "Bir sayfanın SÖZLEŞMESİNİ döndürür: katalogdaki ekran elemanları (kimlik + etiket) ve sayfa bir " +
    "endpoint'e gönderiyorsa o endpoint'in Swagger şemasından alan adları, tipleri, zorunluluklar ve kurallar. " +
    "Alan etiketi, kullanım talimatı veya iş kuralı İÇERMEZ; 'nasıl yapılır / neden' için search_app_knowledge kullan. " +
    "Kullanıcının BULUNMADIĞI bir sayfanın alanlarını bilmen gerektiğinde veya o sayfaya yazmadan/işaretlemeden " +
    "önce kullan. Bulunduğu sayfanın elemanları ve etiketleri zaten ekran özetinde gelir.")]
    public static PageSchemaResult GetPageSchema(
        AppCatalogStore catalog,
        [Description("Sayfa kimliği (örn: 'student-create') veya path (örn: '/teacher').")]
        string page)
    {
        var definition = catalog.FindPage(page) ?? throw UnknownPage(catalog, page);

        return new PageSchemaResult
        {
            Page = definition,
            Form = catalog.FindForm(definition.FormId)
        };
    }

    [McpServerTool(UseStructuredContent = true)]
    [Description(
    "Kullanıcıyı uygulamadaki başka bir sayfaya yönlendirmek için navigation action üretir. " +
    "Sayfa kimliği (örn: 'student-create') veya path verilebilir; yalnızca katalogdaki sayfalar geçerlidir.")]
    public static NavigationResult NavigateToPage(
        AppCatalogStore catalog,
        [Description("Hedef sayfa kimliği veya path'i.")]
        string page)
    {
        // Alias'lar da kabul edilir ama istemciye her zaman kanonik path gider.
        var definition = catalog.FindPage(page) ?? throw UnknownPage(catalog, page);

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
    private static McpException UnknownPage(AppCatalogStore catalog, string page) =>
        new($"Geçersiz sayfa: '{page}'. Geçerli sayfalar: " +
            string.Join(", ", catalog.Catalog.Pages.Select(item => $"{item.Id} ({item.Path})")));
}
