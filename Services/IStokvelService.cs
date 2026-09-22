using RondiTrack.DTOs.Contributions;

namespace RondiTrack.Services;

/// <summary>
/// Defines business operations involving stokvel membership
/// and contribution recording.
/// </summary>
public interface IStokvelService
{
    /// <summary>
    /// Applies the business rules required to add a user to a stokvel.
    /// </summary>
    Task<AddMemberResult> AddMemberAsync(
        Guid stokvelId,
        Guid userId);

    /// <summary>
    /// Applies the business rules required to remove a user from a stokvel.
    /// </summary>
    Task<bool> RemoveMemberAsync(
        Guid stokvelId,
        Guid userId);

    /// <summary>
    /// Records a contribution while enforcing duplicate and
    /// idempotency rules.
    /// </summary>
    Task<RecordContributionResult> RecordContributionAsync(
        Guid stokvelId,
        Guid userId,
        string idempotencyKey,
        RecordContributionRequest request);
}