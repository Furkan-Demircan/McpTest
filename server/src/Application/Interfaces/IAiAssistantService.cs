namespace Application.AI;

public interface IAiAssistantService
{
    Task<AiChatResponse> ChatAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// İstemcinin uyguladığı UI aksiyonlarının sonuçlarıyla bekleyen turu sürdürür.
    /// Tur bulunamazsa (süresi dolmuş/zaten kullanılmış) null döner.
    /// </summary>
    Task<AiChatResponse?> ContinueAsync(
        AiContinueRequest request,
        CancellationToken cancellationToken = default);
}