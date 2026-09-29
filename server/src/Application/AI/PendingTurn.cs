using Application.AI.Contracts;

namespace Application.AI;

/// <summary>
/// İstemcinin UI aksiyonlarını uygulamasını bekleyen yarım kalmış bir sohbet turu.
/// Tool çağrıları yapılmış ama UI tool'larının sonuçları henüz modele yazılmamıştır.
/// </summary>
public class PendingTurn
{
    public string? CurrentPage { get; set; }

    // LLM mesaj geçmişi (assistant tool_call mesajları ve sunucuda çalışan tool sonuçları dahil)
    public List<ChatMessage> Messages { get; set; } = [];

    // Sonucu istemciden beklenen UI tool çağrıları
    public List<PendingToolCall> Pending { get; set; } = [];

    public List<AiTraceStep> Trace { get; set; } = [];

    // İstemciye daha önce gönderilmiş trace adımı sayısı (her cevapta sadece yeniler gider)
    public int TraceSentCount { get; set; }

    // Tüm devam adımları boyunca toplam LLM turu (tur limiti buna uygulanır)
    public int IterationsUsed { get; set; }
}

public record PendingToolCall(string ToolCallId, string ToolName, AiAction Action);
