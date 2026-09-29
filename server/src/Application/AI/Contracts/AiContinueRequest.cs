using System.Text.Json;
using Application.AI.Contracts;

namespace Application.AI;

/// <summary>
/// İstemcinin, bekleyen UI aksiyonlarını uyguladıktan sonra gönderdiği devam isteği.
/// Gerçek sonuçlar ilgili tool çağrılarının sonucu olarak modele yazılır.
/// </summary>
public class AiContinueRequest
{
    public string ContinuationId { get; set; } = string.Empty;

    public List<AiActionResult> Results { get; set; } = [];

    // Aksiyonlar uygulandıktan sonraki ekran (örn. navigasyondan sonra yeni sayfa)
    public ScreenSnapshot? Screen { get; set; }
}

public class AiActionResult
{
    public string ToolCallId { get; set; } = string.Empty;

    // "applied" | "partial" | "failed"
    public string Status { get; set; } = string.Empty;

    // Aksiyona özgü ayrıntı, örn. fill_fields için { written, notFound, notWritable }
    public JsonElement? Detail { get; set; }

    public string? Error { get; set; }
}
