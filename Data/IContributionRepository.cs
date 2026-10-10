using RondiTrack.Models;

namespace RondiTrack.Data;

public interface IContributionRepository
{
    Task<Contribution?> GetByMemberAndCycleAsync(
        Guid stokvelId,
        Guid userId,
        int cycle);

    Task<Contribution> AddAsync(
        Contribution contribution);

    Task<IEnumerable<Contribution>> GetByStokvelAsync(
        Guid stokvelId);

    Task<IReadOnlyList<Contribution>> GetPageByCycleAsync(
        Guid stokvelId,
        int cycleNumber,
        ContributionPageQuery query);
}