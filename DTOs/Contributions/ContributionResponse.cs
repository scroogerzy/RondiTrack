namespace RondiTrack.DTOs.Contributions;

/// <summary>
/// Represents the public HTTP response for a contribution.
/// </summary>
public record ContributionResponse(
    Guid Id,
    Guid StokvelId,
    Guid UserId,
    int Cycle,
    decimal Amount,
    DateTime RecordedAt);
