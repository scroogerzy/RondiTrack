namespace RondiTrack.Infrastructure.Data;

/// <summary>
/// PostgreSQL-backed idempotency receipt. A reservation and its response are
/// written in the same database transaction as the contribution.
/// </summary>
public sealed class IdempotencyEntry
{
    public string IdempotencyKey { get; set; } = string.Empty;
    public string RequestHash { get; set; } = string.Empty;
    public string? ResponseJson { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
