namespace RondiTrack.DTOs.Contributions;

/// <summary>
/// Represents the data required to record a stokvel contribution.
/// </summary>
public record RecordContributionRequest(
    int Cycle,
    decimal Amount);
