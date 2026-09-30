using System.Collections.Concurrent;

namespace API.Tools;

/// <summary>
/// Kullanıcıya önizlemesi gösterilmiş, onay bekleyen yazma işlemleri (örn. save_student).
/// Onaylı çağrı ancak aynı değerler önceden önizlendiyse uygulanır. Tek sunucu örneği içindir.
/// </summary>
public class PendingConfirmationStore
{
    private static readonly TimeSpan TimeToLive = TimeSpan.FromMinutes(10);

    private readonly ConcurrentDictionary<string, DateTimeOffset> _pending = new();

    public void Add(string key)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var (pendingKey, expiresAt) in _pending)
        {
            if (expiresAt <= now)
            {
                _pending.TryRemove(pendingKey, out _);
            }
        }

        _pending[key] = now.Add(TimeToLive);
    }

    public bool TryTake(string key) =>
        _pending.TryRemove(key, out var expiresAt) && expiresAt > DateTimeOffset.UtcNow;
}
