using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RondiTrack.DTOs.Contributions;
using RondiTrack.Infrastructure.Data;

namespace RondiTrack.API.Data;

public sealed class EfIdempotencyStore : IIdempotencyStore
{
    private readonly RondiTrackDbContext _db;
    private readonly TimeProvider _clock;

    public EfIdempotencyStore(RondiTrackDbContext db, TimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<T> ExecuteAtomicallyAsync<T>(Func<Task<T>> operation)
    {
        if (!_db.Database.IsRelational())
            return await operation();

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var result = await operation();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IdempotencyRecord?> FindAsync(string key)
    {
        var entry = await _db.IdempotencyEntries
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.IdempotencyKey == key);

        if (entry is null)
            return null;

        var response = entry.ResponseJson is null
            ? null
            : JsonSerializer.Deserialize<ContributionResponse>(entry.ResponseJson);

        return new IdempotencyRecord(entry.RequestHash, response);
    }

    public async Task<bool> TryReserveAsync(string key, string requestHash)
    {
        var now = _clock.GetUtcNow().UtcDateTime;

        if (!_db.Database.IsRelational())
        {
            var alreadyExists = await _db.IdempotencyEntries
                .AnyAsync(item => item.IdempotencyKey == key);
            if (alreadyExists)
                return false;

            _db.IdempotencyEntries.Add(new IdempotencyEntry
            {
                IdempotencyKey = key,
                RequestHash = requestHash,
                CreatedAtUtc = now
            });
            await _db.SaveChangesAsync();
            return true;
        }

        var inserted = await _db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "IdempotencyEntries"
                ("IdempotencyKey", "RequestHash", "ResponseJson", "CreatedAtUtc")
            VALUES ({key}, {requestHash}, NULL, {now})
            ON CONFLICT ("IdempotencyKey") DO NOTHING;
            """);

        return inserted == 1;
    }

    public async Task SaveAsync(string key, IdempotencyRecord record)
    {
        var entry = await _db.IdempotencyEntries
            .SingleOrDefaultAsync(item => item.IdempotencyKey == key);

        if (entry is null)
            throw new InvalidOperationException(
                "The idempotency key must be reserved before its response is saved.");

        entry.RequestHash = record.RequestHash;
        entry.ResponseJson = record.ResponseBody is null
            ? null
            : JsonSerializer.Serialize(record.ResponseBody);

        await _db.SaveChangesAsync();
    }

    public async Task<int> RemoveOlderThanAsync(DateTime cutoffUtc)
    {
        var expired = await _db.IdempotencyEntries
            .Where(item => item.CreatedAtUtc < cutoffUtc)
            .ToListAsync();

        if (expired.Count == 0)
            return 0;

        _db.IdempotencyEntries.RemoveRange(expired);
        return await _db.SaveChangesAsync();
    }
}
