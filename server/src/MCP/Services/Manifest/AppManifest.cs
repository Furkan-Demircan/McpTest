namespace MCP.Server.Manifest;

// Uygulama manifest'i: sayfalar, formlar, alanlar ve ekran elemanları.
// Kaynak backend'dir: API host bunu [AppForm] işaretli controller action'larından ve
// DTO attribute'larından runtime'da üretir (API/Forms/AppManifestBuilder) ve
// GET /api/app-manifest ile istemciye sunar. Elle düzenlenen bir dosya yoktur.

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
    public string Module { get; set; } = "Genel";

    // Doluysa ana sayfa menüsünde bu etiketle görünür (nav-{id} elemanı)
    public string? NavLabel { get; set; }
    public string? FormId { get; set; }
    public List<PageElement> Elements { get; set; } = [];
}

public class PageElement
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;

    // Menü linkleri için hedef sayfa
    public string? TargetPageId { get; set; }
}

public class FormDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PageId { get; set; } = string.Empty;
    public string Module { get; set; } = "Genel";
    public FormSubmit Submit { get; set; } = new();
    public string SubmitElementId { get; set; } = string.Empty;
    public string ResetElementId { get; set; } = string.Empty;
    public List<FieldDefinition> Fields { get; set; } = [];
}

public class FormSubmit
{
    public string Method { get; set; } = "POST";
    public string Url { get; set; } = string.Empty;
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
    public List<string>? Options { get; set; }
    public string? Hint { get; set; }
}

public class FieldRule
{
    // "minLength" | "maxLength" | "pattern" | "email"
    public string Kind { get; set; } = string.Empty;
    public int? Value { get; set; }
    public string? Pattern { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>Ekrandaki bir elemanın manifest'teki karşılığı (sayfa elemanı veya form alanı).</summary>
public record ManifestElement(string Id, string Label, string Kind, string PageId);
