namespace RondiTrack.DTOs.Contributions;

/// <summary>Request to record a contribution for a specific cycle using integer cents.</summary>
public sealed record RecordCycleContributionRequest(
    Guid UserId,
    long AmountInCents);
