using System.Collections.Concurrent;
using Application.AI;

namespace Infrastructure.AI;

/// <summary>
/// Bekleyen turları süreç içi bellekte tutar. Tek sunucu örneği içindir: uygulama
/// yeniden başlarsa veya istek başka bir örneğe düşerse tur bulunamaz (410).
/// </summary>
public class InMemoryPendingTurnStore : IPendingTurnStore
{
    private static readonly TimeSpan TimeToLive = TimeSpan.FromMinutes(5);

    private readonly ConcurrentDictionary<string, (PendingTurn Turn, DateTimeOffset ExpiresAt)> _turns = new();

    public string Save(PendingTurn turn)
    {
        RemoveExpired();

        var id = Guid.NewGuid().ToString("N");
        _turns[id] = (turn, DateTimeOffset.UtcNow.Add(TimeToLive));
        return id;
    }

    public PendingTurn? Take(string continuationId) =>
        _turns.TryRemove(continuationId, out var entry) && entry.ExpiresAt > DateTimeOffset.UtcNow
            ? entry.Turn
            : null;

    private void RemoveExpired()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var (id, entry) in _turns)
        {
            if (entry.ExpiresAt <= now)
            {
                _turns.TryRemove(id, out _);
            }
        }
    }
}
