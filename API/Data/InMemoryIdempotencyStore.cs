using System.Collections.Concurrent;

namespace RondiTrack.API.Data;

/// <summary>
/// In-memory idempotency store for isolated unit tests only.
/// Production uses EfIdempotencyStore backed by PostgreSQL.
/// </summary>
public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private sealed record Entry(IdempotencyRecord Record, DateTime CreatedAtUtc);

    private readonly ConcurrentDictionary<string, Entry> _records = new();
    private readonly TimeProvider _clock;
    private readonly SemaphoreSlim _operationGate = new(1, 1);

    public InMemoryIdempotencyStore(TimeProvider clock) => _clock = clock;

    public Task<IdempotencyRecord?> FindAsync(string key)
    {
        return Task.FromResult(
            _records.TryGetValue(key, out var entry) ? entry.Record : null);
    }

    public Task<bool> TryReserveAsync(string key, string requestHash)
    {
        var now = _clock.GetUtcNow().UtcDateTime;
        return Task.FromResult(_records.TryAdd(
            key,
            new Entry(new IdempotencyRecord(requestHash, null), now)));
    }

    public async Task<T> ExecuteAtomicallyAsync<T>(Func<Task<T>> operation)
    {
        await _operationGate.WaitAsync();
        try
        {
            return await operation();
        }
        finally
        {
            _operationGate.Release();
        }
    }

    public Task SaveAsync(string key, IdempotencyRecord record)
    {
        var now = _clock.GetUtcNow().UtcDateTime;
        _records.AddOrUpdate(
            key,
            _ => new Entry(record, now),
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

            var collection = (ICollection<KeyValuePair<string, Entry>>)_records;
            if (collection.Remove(pair))
                removed++;
        }

        return Task.FromResult(removed);
    }
}
