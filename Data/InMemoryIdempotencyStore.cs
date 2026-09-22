using System.Collections.Concurrent;

namespace RondiTrack.Data;

/// <summary>
/// In-memory implementation of the idempotency store.
/// </summary>
public class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, IdempotencyRecord?> _records = new();

    /// <inheritdoc />
    public Task<IdempotencyRecord?> FindAsync(string key)
    {
        var found = _records.TryGetValue(key, out var record);

        return Task.FromResult(
            found ? record : null);
    }

    /// <inheritdoc />
    public Task<bool> TryReserveAsync(string key)
    {
        // A null value represents a key that has been reserved
        // but whose operation has not completed yet.
        return Task.FromResult(
            _records.TryAdd(key, null));
    }

    /// <inheritdoc />
    public Task SaveAsync(
        string key,
        IdempotencyRecord record)
    {
        _records[key] = record;

        return Task.CompletedTask;
    }
}