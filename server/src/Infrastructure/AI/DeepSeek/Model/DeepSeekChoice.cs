namespace Infrastructure.AI.DeepSeek.Models;

public class DeepSeekChoice
{
    public DeepSeekMessage Message { get; set; } = new();

    public string? FinishReason { get; set; }
}