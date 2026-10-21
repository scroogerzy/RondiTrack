namespace RondiTrack.DTOs.ContributionCycles;

public record ContributionCycleResponse(
    Guid Id,
    Guid StokvelId,
    int PeriodNumber,
    DateTime StartDate,
    DateTime EndDate,
    decimal TargetAmount,
    uint Version,
    DateTime? PayoutProcessedAtUtc,
    DateTime? ClosedAtUtc,
    string Status,
    long TotalCollectedCents,
    long AmountDueCents);
