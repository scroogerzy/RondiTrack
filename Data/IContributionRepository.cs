using RondiTrack.Models;

namespace RondiTrack.Data;

/// <summary>
/// Defines persistence operations for contribution records.
/// </summary>
public interface IContributionRepository
{
    /// <summary>
    /// Finds a contribution made by a specific user for a stokvel cycle.
    /// </summary>
    Task<Contribution?> GetByMemberAndCycleAsync(
        Guid stokvelId,
        Guid userId,
        int cycle);

    /// <summary>
    /// Adds a new contribution to the repository.
    /// </summary>
    Task<Contribution> AddAsync(Contribution contribution);

    /// <summary>
    /// Gets all contributions recorded for a stokvel.
    /// </summary>
    Task<IEnumerable<Contribution>> GetByStokvelAsync(Guid stokvelId);
}