using Application.AI.Contracts;

namespace Application.AI;

public class AiRequest
{
    public string? CurrentPage { get; set; }

    public ScreenSnapshot? Screen { get; set; }

    // Ekran özeti asistanın kendi aksiyonlarından sonra mı alındı (devam adımı)?
    // Öyleyse özetteki değerlerin bir kısmını asistan az önce yazmıştır.
    public bool ScreenAfterClientActions { get; set; }

    public List<ChatMessage> Messages { get; set; } = [];

    public List<AiToolDefinition> Tools { get; set; } = [];
}
