namespace Application.AI.Contracts;

/// <summary>
/// Bir chat isteği sırasında gerçekleşen tek bir adım (LLM çağrısı veya tool çağrısı).
/// Keşif/debug amaçlı istemciye döner.
/// </summary>
public class AiTraceStep
{
    // "llm" | "tool" | "client" (istemcinin bildirdiği aksiyon sonucu) | "limit"
    public string Kind { get; set; } = string.Empty;

    public int Iteration { get; set; }

    public string? Name { get; set; }

    public string? Arguments { get; set; }

    public string? Result { get; set; }

    public bool IsError { get; set; }

    public long DurationMs { get; set; }
}
