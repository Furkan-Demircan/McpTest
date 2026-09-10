using Application.AI.Contracts;

namespace Application.AI;

public class AiChatResponse
{
    public string Message { get; set; } = string.Empty;
    public List<string> MissingFields { get; set; } = [];

    public List<AiAction> Actions { get; set; } = [];
}