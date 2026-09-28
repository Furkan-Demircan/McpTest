namespace MCP.Server.Manifest;

// Uygulama manifest'i: asistanın uygulamayı tanıması için sayfalar, formlar ve alanlar.
// UI'ı çizmek için değil, asistan için. API host iki kaynağı birleştirir
// (API/Forms/AppManifestBuilder):
// - Sayfa kataloğu (client/src/app/aiPages.ts → Manifest/app-pages.json): sayfalar,
//   path'ler, sayfa elemanları ve sayfanın gönderdiği endpoint
// - Swagger: endpoint'in request body şemasından formun alanları ve kuralları

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

    // Sayfanın gönderdiği endpoint, örn. "POST /api/Users"; alanları Swagger'dan çözülür
    public string? Endpoint { get; set; }
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
    public string Module { get; set; } = "Genel";
    public FormSubmit Submit { get; set; } = new();
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

    // Ekranda alan bu adla bulunur: data-ai-field → name → id
    public string ElementId { get; set; } = string.Empty;
    public bool Required { get; set; }
    public List<FieldRule>? Rules { get; set; }
    public string? Hint { get; set; }
}

public class FieldRule
{
    // "minLength" | "maxLength" | "pattern" | "email"
    public string Kind { get; set; } = string.Empty;
    public int? Value { get; set; }
    public string? Pattern { get; set; }
}

/// <summary>Ekrandaki bir elemanın manifest'teki karşılığı (sayfa elemanı veya form alanı).</summary>
public record ManifestElement(string Id, string Label, string Kind, string PageId);
