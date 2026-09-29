namespace MCP.Server.Catalog;

// Uygulama kataloğu: asistanın uygulamayı tanıması için sayfalar, formlar ve alanlar.
// UI'ı çizmek için değil, asistan için. API host iki kaynağı birleştirir
// (API/Catalog/AppCatalogBuilder):
// - Sayfa kataloğu (client/src/app/aiPages.ts → Catalog/app-pages.json): sayfalar,
//   path'ler, sayfa elemanları ve sayfanın gönderdiği endpoint
// - Swagger (sadece sözleşme): endpoint'in request body'sindeki alan adları, tipler,
//   zorunluluk ve kurallar
// Kullanım bilgisi (nasıl yapılır, iş kuralları, alan etiketleri) burada değil,
// Application Knowledge katmanındadır (Knowledge/*.md, search_app_knowledge).

public class AppCatalog
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
    public List<FieldDefinition> Fields { get; set; } = [];
}

// Sözleşme alanı (Swagger). Etiket yoktur: kullanıcının gördüğü etiket ekran özetinden,
// kullanım dili rehberlerden gelir.
public class FieldDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    // Ekranda alan bu adla bulunur: data-ai-field → name → id
    public string ElementId { get; set; } = string.Empty;
    public bool Required { get; set; }
    public List<FieldRule>? Rules { get; set; }
}

public class FieldRule
{
    // "minLength" | "maxLength" | "pattern" | "email" | "minimum" | "maximum"
    public string Kind { get; set; } = string.Empty;
    public int? Value { get; set; }
    public string? Pattern { get; set; }
}

/// <summary>
/// Ekrandaki bir elemanın katalogdaki karşılığı. Katalog elemanlarının etiketi vardır;
/// form alanlarının (Swagger) etiketi yoktur (null).
/// </summary>
public record CatalogElement(string Id, string? Label, string Kind, string PageId);
