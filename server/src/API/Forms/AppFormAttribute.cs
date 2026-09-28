namespace API.Forms;

/// <summary>
/// Bir controller action'ını uygulama formu olarak işaretler (opt-in).
/// Yalnızca bu attribute'u taşıyan action'lar manifest'e girer: formun alanları
/// action'ın [FromBody] DTO'sundan, submit adresi action'ın route'undan türetilir.
/// İşaretlenmemiş endpoint'ler istemciye ve modele hiç görünmez.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class AppFormAttribute(string id) : Attribute
{
    /// <summary>Form kimliği; asistan fill_form'da target olarak kullanır.</summary>
    public string Id { get; } = id;

    /// <summary>Formun sayfa kimliği; boşsa form kimliği kullanılır.</summary>
    public string? PageId { get; init; }

    /// <summary>Sayfanın kanonik path'i (örn. "/course").</summary>
    public required string Path { get; init; }

    /// <summary>Kanonik path'e yönlendirilen ek adresler.</summary>
    public string[] Aliases { get; init; } = [];

    public required string Title { get; init; }

    public string? Description { get; init; }

    /// <summary>Büyük uygulamada sayfaları gruplar; asistan önce modülü sonra sayfayı bulur.</summary>
    public string Module { get; init; } = "Genel";

    /// <summary>Doluysa ana sayfa menüsünde bu etiketle görünür.</summary>
    public string? NavLabel { get; init; }

    public string SubmitLabel { get; init; } = "Kaydet";
}
