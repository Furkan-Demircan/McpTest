using MCP.Server.Catalog;

namespace MCP.Server.Models;

public class PageSchemaResult
{
    public PageDefinition Page { get; set; } = new();

    // Sayfada form varsa alanları, zorunlulukları, kuralları ve ekran kimlikleri
    public FormDefinition? Form { get; set; }
}

public class AppPagesResult
{
    public int TotalPages { get; set; }
    public List<AppModuleSummary> Modules { get; set; } = [];
    public List<AppPageSummary> Pages { get; set; } = [];
    public string? Hint { get; set; }
}

public class AppModuleSummary
{
    public string Name { get; set; } = string.Empty;
    public int PageCount { get; set; }
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
