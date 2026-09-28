using MCP.Server.Manifest;

namespace MCP.Server.Models;

public class PageSchemaResult
{
    public PageDefinition Page { get; set; } = new();

    // Sayfada form varsa alanları, zorunlulukları, kuralları ve ekran kimlikleri
    public FormDefinition? Form { get; set; }
}

public class AppPagesResult
{
    public List<AppPageSummary> Pages { get; set; } = [];
}

public class AppPageSummary
{
    public string Id { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? FormId { get; set; }
}
