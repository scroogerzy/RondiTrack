using RondiTrack.Models;

namespace RondiTrack.Data;

public interface IContributionCycleRepository
{
    Task<IEnumerable<ContributionCycle>> GetByStokvelAsync(
        Guid stokvelId);

    Task<ContributionCycle?> GetByIdAsync(
        Guid id);

    Task<ContributionCycle?> GetByStokvelAndPeriodAsync(
        Guid stokvelId,
        int periodNumber);

    Task<ContributionCycle> AddAsync(
        ContributionCycle cycle);

    Task UpdateAsync(
        ContributionCycle cycle);

    Task<bool> DeleteAsync(
        Guid id);
}