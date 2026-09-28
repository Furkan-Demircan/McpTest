namespace Application.AI.Contracts;

/// <summary>
/// İstemcinin her istekte gönderdiği, kullanıcının o an gördüğü ekranın özeti (DOM'dan).
/// Asistan bulunulan sayfayı buradan okur; uygulamanın state'ine erişmez.
/// </summary>
public class ScreenSnapshot
{
    public string? Heading { get; set; }
    public List<ScreenElement> Elements { get; set; } = [];
}

public class ScreenElement
{
    public string Id { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;

    // Alan eşlemesi: data-ai-field (Field) → name → id
    public string? Field { get; set; }
    public string? Name { get; set; }

    // Kısaltılmış mevcut değer; şifre alanlarında hiç gönderilmez
    public string? Value { get; set; }
    public bool? Required { get; set; }
    public bool? Disabled { get; set; }
}
