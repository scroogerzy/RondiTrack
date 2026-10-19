namespace RondiTrack.Domain.Models;

/// <summary>
/// Records a member's unpaid contribution shortfall for a closed cycle.
/// ShortfallCents is stored as an integer to avoid fractional-cent errors.
/// </summary>
public sealed class ArrearRecord
{
    public Guid Id { get; private set; }
    public Guid StokvelId { get; private set; }
    public Guid UserId { get; private set; }
    public int CyclePeriodNumber { get; private set; }
    public long ShortfallCents { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }

    private ArrearRecord()
    {
    }

    public ArrearRecord(
        Guid stokvelId,
        Guid userId,
        int cyclePeriodNumber,
        long shortfallCents,
        DateTime recordedAtUtc)
    {
        if (stokvelId == Guid.Empty)
            throw new ArgumentException("Stokvel ID is required.", nameof(stokvelId));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required.", nameof(userId));

        if (cyclePeriodNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(cyclePeriodNumber));

        if (shortfallCents <= 0)
            throw new ArgumentOutOfRangeException(nameof(shortfallCents));

        if (recordedAtUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Timestamp must be UTC.", nameof(recordedAtUtc));

        Id = Guid.NewGuid();
        StokvelId = stokvelId;
        UserId = userId;
        CyclePeriodNumber = cyclePeriodNumber;
        ShortfallCents = shortfallCents;
        RecordedAtUtc = recordedAtUtc;
    }
}
