using RondiTrack.Models;

namespace RondiTrack.Data;

/// <summary>
/// In-memory implementation of the contribution repository.
/// </summary>
public class InMemoryContributionRepository : IContributionRepository
{
    private readonly List<Contribution> _contributions = new();

    /// <inheritdoc />
    public Task<Contribution?> GetByMemberAndCycleAsync(
        Guid stokvelId,
        Guid userId,
        int cycle)
    {
        var contribution = _contributions.FirstOrDefault(c =>
            c.StokvelId == stokvelId &&
            c.UserId == userId &&
            c.Cycle == cycle);

        return Task.FromResult(contribution);
    }

    /// <inheritdoc />
    public Task<Contribution> AddAsync(Contribution contribution)
    {
        _contributions.Add(contribution);

        return Task.FromResult(contribution);
    }

    /// <inheritdoc />
    public Task<IEnumerable<Contribution>> GetByStokvelAsync(
        Guid stokvelId)
    {
        var contributions = _contributions
            .Where(c => c.StokvelId == stokvelId)
            .ToList();

        return Task.FromResult<IEnumerable<Contribution>>(contributions);
    }
}