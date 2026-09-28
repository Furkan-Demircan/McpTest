using System.Text.Json;
using MCP.Server.Catalog;

namespace API.Catalog;

/// <summary>
/// Asistan kataloğunu iki kaynaktan birleştirir:
/// - Sayfa kataloğu (Catalog/app-pages.json; kaynağı client/src/app/aiPages.ts): sayfalar,
///   path'ler, sayfa elemanları ve her sayfanın gönderdiği endpoint
/// - Swagger: o endpoint'in request body şemasından formun alanları ve kuralları
/// Frontend'e ve backend'e dokunmaz; UI'ı çizmez. Tutarsızlıklar Issues olarak döner.
/// </summary>
public sealed class AppCatalogBuilder(SwaggerFormSchemaProvider swagger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public (AppCatalog Catalog, IReadOnlyList<string> Issues) Build(string catalogPath)
    {
        var issues = new List<string>();

        if (!File.Exists(catalogPath))
        {
            issues.Add($"Sayfa kataloğu bulunamadı: {catalogPath} (client'ta 'npm run pages' çalıştırın)");
            return (new AppCatalog(), issues);
        }

        var pages = JsonSerializer.Deserialize<AppCatalog>(File.ReadAllText(catalogPath), JsonOptions)?.Pages ?? [];
        var forms = new List<FormDefinition>();

        foreach (var page in pages.Where(page => !string.IsNullOrWhiteSpace(page.Endpoint)))
        {
            var fields = swagger.GetRequestFields(page.Endpoint!);
            if (fields is null)
            {
                issues.Add($"Sayfa '{page.Id}': endpoint '{page.Endpoint}' Swagger'da bulunamadı veya JSON body'si yok.");
                continue;
            }

            page.FormId = page.Id;
            forms.Add(new FormDefinition
            {
                Id = page.Id,
                Title = page.Title,
                PageId = page.Id,
                Module = page.Module,
                Fields = fields
            });
        }

        var catalog = new AppCatalog { Pages = pages, Forms = forms };
        issues.AddRange(Validate(catalog));

        return (catalog, issues);
    }

    private static IEnumerable<string> Validate(AppCatalog catalog)
    {
        static IEnumerable<string> Duplicates(IEnumerable<string> values) =>
            values.GroupBy(value => value, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);

        foreach (var id in Duplicates(catalog.Pages.Select(page => page.Id)))
            yield return $"Tekrarlanan sayfa kimliği: {id}";

        foreach (var path in Duplicates(catalog.Pages.SelectMany(page => page.Aliases.Prepend(page.Path))))
            yield return $"Tekrarlanan path/alias: {path}";

        // Aynı alan adı farklı sayfalarda olabilir; tekrar sadece aynı sayfa içinde hatadır
        foreach (var page in catalog.Pages)
        {
            var fields = catalog.Forms.FirstOrDefault(form => form.PageId == page.Id)?.Fields.Select(field => field.ElementId) ?? [];

            foreach (var id in Duplicates(page.Elements.Select(element => element.Id).Concat(fields)))
                yield return $"Sayfa '{page.Id}': tekrarlanan eleman kimliği {id}";
        }
    }
}
