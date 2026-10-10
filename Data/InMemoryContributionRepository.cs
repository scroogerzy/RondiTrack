using RondiTrack.Models;

namespace RondiTrack.Data;

public class InMemoryContributionRepository : IContributionRepository
{
    private readonly List<Contribution> _contributions = new();

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

    public Task<Contribution> AddAsync(
        Contribution contribution)
    {
        _contributions.Add(contribution);

        return Task.FromResult(contribution);
    }

    public Task<IEnumerable<Contribution>> GetByStokvelAsync(
        Guid stokvelId)
    {
        var contributions = _contributions
            .Where(c => c.StokvelId == stokvelId)
            .ToList();

        return Task.FromResult<IEnumerable<Contribution>>(
            contributions);
    }
    public Task<IReadOnlyList<Contribution>> GetPageByCycleAsync(
        Guid stokvelId,
        int cycleNumber,
        ContributionPageQuery query)
    {
        IEnumerable<Contribution> contributions = _contributions.Where(c =>
            c.StokvelId == stokvelId && c.Cycle == cycleNumber);

        if (query.UserId.HasValue)
            contributions = contributions.Where(c => c.UserId == query.UserId.Value);

        if (query.LastRecordedAt.HasValue && query.LastId.HasValue)
        {
            var recordedAt = query.LastRecordedAt.Value;
            var id = query.LastId.Value;
            contributions = query.Descending
                ? contributions.Where(c => c.RecordedAt < recordedAt || (c.RecordedAt == recordedAt && c.Id.CompareTo(id) < 0))
                : contributions.Where(c => c.RecordedAt > recordedAt || (c.RecordedAt == recordedAt && c.Id.CompareTo(id) > 0));
        }

        contributions = query.Descending
            ? contributions.OrderByDescending(c => c.RecordedAt).ThenByDescending(c => c.Id)
            : contributions.OrderBy(c => c.RecordedAt).ThenBy(c => c.Id);

        return Task.FromResult<IReadOnlyList<Contribution>>(
            contributions.Take(query.PageSize + 1).ToList());
    }

}