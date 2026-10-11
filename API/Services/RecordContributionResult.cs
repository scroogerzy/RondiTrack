using RondiTrack.DTOs.Contributions;

namespace RondiTrack.Services;

/// <summary>
/// Represents the possible outcomes of recording a contribution.
/// </summary>
public abstract record RecordContributionResult
{
    /// <summary>
    /// Indicates that a new contribution was successfully recorded.
    /// </summary>
    public sealed record Recorded(
        ContributionResponse Contribution)
        : RecordContributionResult;

    /// <summary>
    /// Indicates that the request was successfully replayed using
    /// the stored idempotent response.
    /// </summary>
    public sealed record ReplayedFromCache(
        ContributionResponse Contribution)
        : RecordContributionResult;

    /// <summary>
    /// Indicates that an Idempotency-Key was reused with a different request.
    /// </summary>
    public sealed record KeyConflict(
        string IdempotencyKey)
        : RecordContributionResult;

    /// <summary>
    /// Indicates that the stokvel could not be found.
    /// </summary>
    public sealed record StokvelNotFound(
        Guid StokvelId)
        : RecordContributionResult;

    /// <summary>
    /// Indicates that the user could not be found.
    /// </summary>
    public sealed record UserNotFound(
        Guid UserId)
        : RecordContributionResult;

    /// <summary>
    /// Indicates that the user is not a member of the stokvel.
    /// </summary>
    public sealed record UserNotMember(
        Guid StokvelId,
        Guid UserId)
        : RecordContributionResult;

    /// <summary>
    /// Indicates that a contribution already exists for the member and cycle.
    /// </summary>
    public sealed record DuplicateContribution(
        Guid StokvelId,
        Guid UserId,
        int Cycle)
        : RecordContributionResult;

    /// <summary>
    /// Indicates that the contribution amount violates a business rule.
    /// </summary>
    public sealed record InvalidAmount(
        decimal Amount)
        : RecordContributionResult;

    /// <summary>
    /// Indicates that the contribution cycle is invalid.
    /// </summary>
    public sealed record InvalidCycle(
        int Cycle)
        : RecordContributionResult;
}