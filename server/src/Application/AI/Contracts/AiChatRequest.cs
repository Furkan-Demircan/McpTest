using Application.AI.Contracts;

namespace Application.AI;

public class AiChatRequest
{
    public string? CurrentPage { get; set; }

    // Kullanıcının bulunduğu sayfadaki formun kimliği (manifest form.id); formsuz sayfada null
    public string? ActiveFormId { get; set; }

    public ScreenSnapshot? Screen { get; set; }

    public List<ChatMessage> Messages { get; set; } = [];

    public Dictionary<string, string?> FormData { get; set; } = [];
}
