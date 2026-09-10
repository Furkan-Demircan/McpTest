namespace Application.AI;

public class AiChatResponse
{
    public string Message { get; set; } = string.Empty;

    public Dictionary<string, string?> FormPatch { get; set; } = new();

    public List<string> MissingFields { get; set; } = [];
}