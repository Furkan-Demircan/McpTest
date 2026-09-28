namespace MCP.Server.Manifest;

// client/src/app/appManifest.ts'in sunucu tarafı karşılığı.
// Kaynak TS dosyasıdır; JSON `npm run manifest` ile üretilir, elle düzenlenmez.

public class AppManifest
{
    public List<PageDefinition> Pages { get; set; } = [];
    public List<FormDefinition> Forms { get; set; } = [];
}

public class PageDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public List<string> Aliases { get; set; } = [];
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? FormId { get; set; }
    public List<PageElement> Elements { get; set; } = [];
}

public class PageElement
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
}

public class FormDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PageId { get; set; } = string.Empty;
    public string SubmitElementId { get; set; } = string.Empty;
    public string ResetElementId { get; set; } = string.Empty;
    public List<FieldDefinition> Fields { get; set; } = [];
}

public class FieldDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ElementId { get; set; } = string.Empty;
    public bool Required { get; set; }
    public string? RequiredMessage { get; set; }
    public List<FieldRule>? Rules { get; set; }
    public string? Hint { get; set; }
}

public class FieldRule
{
    public string Kind { get; set; } = string.Empty;
    public int? Value { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>Ekrandaki bir elemanın manifest'teki karşılığı (sayfa elemanı veya form alanı).</summary>
public record ManifestElement(string Id, string Label, string Kind, string PageId);
