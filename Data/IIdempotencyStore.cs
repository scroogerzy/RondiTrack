using RondiTrack.DTOs.Contributions;

namespace RondiTrack.Data;

/// <summary>
/// Stores the result of an idempotent operation so that
/// retries can return the original response.
/// </summary>
public interface IIdempotencyStore
{
    /// <summary>
    /// Finds an existing idempotency record using its key.
    /// </summary>
    Task<IdempotencyRecord?> FindAsync(string key);

    /// <summary>
    /// Atomically reserves an idempotency key.
    /// </summary>
    Task<bool> TryReserveAsync(string key);

    /// <summary>
    /// Stores the request hash and original response for an idempotency key.
    /// </summary>
    Task SaveAsync(
        string key,
        IdempotencyRecord record);
}

/// <summary>
/// Represents the information stored for an idempotent request.
/// </summary>
public record IdempotencyRecord(
    string RequestHash,
    ContributionResponse? ResponseBody);