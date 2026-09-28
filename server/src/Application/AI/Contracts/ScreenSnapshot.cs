namespace Application.AI.Contracts;

/// <summary>
/// İstemcinin her istekte gönderdiği, kullanıcının o an gördüğü ekranın özeti.
/// Değer içermez; sadece etkileşimli elemanlar ve dolu/boş bilgisi.
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
    public bool? Required { get; set; }
    public bool? Filled { get; set; }
    public bool? Disabled { get; set; }
}
