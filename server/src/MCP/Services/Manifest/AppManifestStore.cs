namespace MCP.Server.Manifest;

/// <summary>
/// Uygulama manifest'i üzerinde sayfa/form/eleman aramaları.
/// Sayfalar kimlik, kanonik path veya alias ile bulunabilir.
/// </summary>
public class AppManifestStore
{
    private readonly Dictionary<string, ManifestElement> _elements;
    private readonly List<(PageDefinition Page, HashSet<string> Tokens)> _pageIndex;

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

        _pageIndex = manifest.Pages
            .Select(page => (page, TurkishText.Tokenize(string.Join(' ',
                new[] { page.Title, page.Description, page.NavLabel ?? string.Empty, page.Module }
                    .Concat(FindForm(page.FormId)?.Fields.Select(field => field.Label) ?? [])))))
            .ToList();
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

    public IReadOnlyList<(string Module, int PageCount)> Modules() =>
        Manifest.Pages
            .GroupBy(page => page.Module)
            .Select(group => (group.Key, group.Count()))
            .ToList();

    /// <summary>
    /// Sayfaları sorguya göre sıralar (başlık, açıklama, menü etiketi, modül, alan etiketleri).
    /// Sorgu boşsa modül filtresine uyan sayfaları sırasıyla döner.
    /// </summary>
    public IReadOnlyList<PageDefinition> SearchPages(string? query, string? module, int maxResults)
    {
        var candidates = _pageIndex.Where(entry =>
            string.IsNullOrWhiteSpace(module) ||
            entry.Page.Module.Equals(module, StringComparison.OrdinalIgnoreCase));

        var queryTokens = string.IsNullOrWhiteSpace(query) ? [] : TurkishText.QueryTokens(query);

        if (queryTokens.Count == 0)
        {
            return candidates.Select(entry => entry.Page).Take(maxResults).ToList();
        }

        return candidates
            .Select(entry => (entry.Page, Score: queryTokens.Count(token =>
                entry.Tokens.Any(pageToken => TurkishText.IsMatch(token, pageToken)))))
            .Where(match => match.Score > 0)
            .OrderByDescending(match => match.Score)
            .Take(maxResults)
            .Select(match => match.Page)
            .ToList();
    }

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
