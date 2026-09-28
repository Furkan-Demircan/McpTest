using MCP.Server.Manifest;

namespace API.Forms;

/// <summary>
/// Form içermeyen, elle yazılmış özel sayfalar (ana sayfa, liste sayfaları).
/// Form sayfaları buraya yazılmaz; [AppForm] attribute'undan otomatik üretilir.
/// </summary>
public sealed class AppPageRegistry
{
    public List<PageDefinition> Pages { get; } = [];

    public AppPageRegistry Add(PageDefinition page)
    {
        Pages.Add(page);
        return this;
    }
}
