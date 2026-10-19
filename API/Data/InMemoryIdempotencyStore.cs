using System.Collections.Concurrent;

namespace RondiTrack.API.Data;

/// <summary>
/// In-memory idempotency storage. Records expire based on reservation time.
/// Use persistent storage for multi-replica production deployments.
/// </summary>
public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private sealed record Entry(
        IdempotencyRecord? Record,
        DateTime CreatedAtUtc);

    private readonly ConcurrentDictionary<string, Entry> _records = new();
    private readonly TimeProvider _clock;

    public InMemoryIdempotencyStore(TimeProvider clock)
    {
        _clock = clock;
    }

    public Task<IdempotencyRecord?> FindAsync(string key)
    {
        return Task.FromResult(
            _records.TryGetValue(key, out var entry)
                ? entry.Record
                : null);
    }

    public Task<bool> TryReserveAsync(string key)
    {
        var now = _clock.GetUtcNow().UtcDateTime;
        return Task.FromResult(_records.TryAdd(key, new Entry(null, now)));
    }

    public Task SaveAsync(string key, IdempotencyRecord record)
    {
        _records.AddOrUpdate(
            key,
            _ => new Entry(record, _clock.GetUtcNow().UtcDateTime),
            (_, existing) => existing with { Record = record });

        return Task.CompletedTask;
    }

    public Task<int> RemoveOlderThanAsync(DateTime cutoffUtc)
    {
        var removed = 0;

        foreach (var pair in _records)
        {
            if (pair.Value.CreatedAtUtc >= cutoffUtc)
                continue;

            // Remove only the entry we inspected, avoiding removal of a
            // replacement that was concurrently written under the same key.
            var collection =
                (ICollection<KeyValuePair<string, Entry>>)_records;

            if (collection.Remove(pair))
                removed++;
        }

        return Task.FromResult(removed);
    }
}
