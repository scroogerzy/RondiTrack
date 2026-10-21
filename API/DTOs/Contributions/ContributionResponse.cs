namespace RondiTrack.DTOs.Contributions;

/// <summary>Public contribution response including the stable cycle identifier.</summary>
public record ContributionResponse(
    Guid Id,
    Guid StokvelId,
    Guid UserId,
    int Cycle,
    Guid CycleId,
    decimal Amount,
    DateTime RecordedAt);
