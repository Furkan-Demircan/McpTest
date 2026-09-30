namespace Application.AI.Contracts;

/// <summary>
/// Cevabın altında tıklanabilir seçenek olarak gösterilen bir kayıt (örn. aynı adlı öğrencilerden biri).
/// Tool'lar sonuçlarında "choices" döndürür; tıklanınca Message kullanıcı mesajı olarak gönderilir.
/// </summary>
public class AiChoice
{
    public string Label { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public string Message { get; set; } = string.Empty;
}
