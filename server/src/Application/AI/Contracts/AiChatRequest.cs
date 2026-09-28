using Application.AI.Contracts;

namespace Application.AI;

public class AiChatRequest
{
    public string? CurrentPage { get; set; }

    // Kullanıcının o an gördüğü ekran; form değerleri dahil (uygulama state'i değil, DOM)
    public ScreenSnapshot? Screen { get; set; }

    public List<ChatMessage> Messages { get; set; } = [];
}
