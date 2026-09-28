namespace MCP.Server.Manifest;

/// <summary>
/// Uygulama manifest'i üzerinde sayfa/form/eleman aramaları.
/// Sayfalar kimlik, kanonik path veya alias ile bulunabilir.
/// </summary>
public class AppManifestStore
{
    private readonly Dictionary<string, ManifestElement> _elements;

    public AppManifestStore(AppManifest manifest)
    {
        Manifest = manifest;

        // Tekrarlanan kimlikler builder'ın doğrulamasında raporlanır; burada ilki kazanır.
        _elements = manifest.Pages
            .SelectMany(page => page.Elements.Select(element =>
                new ManifestElement(element.Id, element.Label, element.Kind, page.Id)))
            .Concat(manifest.Forms.SelectMany(form => form.Fields.Select(field =>
                new ManifestElement(field.ElementId, field.Label, "field", form.PageId))))
            .GroupBy(element => element.Id, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
    }

    public AppManifest Manifest { get; }

    public PageDefinition? FindPage(string? pageIdOrPath)
    {
        if (string.IsNullOrWhiteSpace(pageIdOrPath))
        {
            return null;
        }

        var key = pageIdOrPath.Trim();
        var path = NormalizePath(key);

        return Manifest.Pages.FirstOrDefault(page =>
            page.Id.Equals(key, StringComparison.OrdinalIgnoreCase) ||
            NormalizePath(page.Path) == path ||
            page.Aliases.Any(alias => NormalizePath(alias) == path));
    }

    public FormDefinition? FindForm(string? formId) =>
        Manifest.Forms.FirstOrDefault(form =>
            form.Id.Equals(formId, StringComparison.OrdinalIgnoreCase));

    public ManifestElement? FindElement(string elementId) =>
        _elements.GetValueOrDefault(elementId);

    private static string NormalizePath(string path)
    {
        var normalized = path.Trim().ToLowerInvariant();
        if (!normalized.StartsWith('/'))
        {
            normalized = "/" + normalized;
        }

        return normalized.Length > 1 ? normalized.TrimEnd('/') : normalized;
    }
}
