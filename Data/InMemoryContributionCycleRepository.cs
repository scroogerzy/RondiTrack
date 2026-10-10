using RondiTrack.Models;

namespace RondiTrack.Data;

/// <summary>
/// In-memory repository used during testing.
/// </summary>
public class InMemoryContributionCycleRepository
    : IContributionCycleRepository
{
    private readonly List<ContributionCycle> _cycles = new();

    public Task<IEnumerable<ContributionCycle>>
        GetByStokvelAsync(Guid stokvelId)
    {
        var result = _cycles
            .Where(c => c.StokvelId == stokvelId)
            .ToList();

        return Task.FromResult<IEnumerable<ContributionCycle>>(result);
    }

    public Task<ContributionCycle?>
        GetByIdAsync(Guid id)
    {
        return Task.FromResult(
            _cycles.FirstOrDefault(c => c.Id == id));
    }

    public Task<ContributionCycle?>
        GetByStokvelAndPeriodAsync(
            Guid stokvelId,
            int periodNumber)
    {
        return Task.FromResult(
            _cycles.FirstOrDefault(c =>
                c.StokvelId == stokvelId &&
                c.PeriodNumber == periodNumber));
    }

    public Task<ContributionCycle>
        AddAsync(ContributionCycle cycle)
    {
        _cycles.Add(cycle);

        return Task.FromResult(cycle);
    }

    public Task UpdateAsync(
        ContributionCycle cycle)
    {
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(
        Guid id)
    {
        var cycle =
            _cycles.FirstOrDefault(c => c.Id == id);

        if (cycle is null)
            return Task.FromResult(false);

        _cycles.Remove(cycle);

        return Task.FromResult(true);
    }
    public Task<IReadOnlyList<ContributionCycle>> GetPageByStokvelAsync(
        Guid stokvelId,
        ContributionCyclePageQuery query)
    {
        IEnumerable<ContributionCycle> cycles = _cycles.Where(c => c.StokvelId == stokvelId);
        if (query.LastPeriodNumber.HasValue && query.LastId.HasValue)
        {
            var period = query.LastPeriodNumber.Value;
            var id = query.LastId.Value;
            cycles = query.Descending
                ? cycles.Where(c => c.PeriodNumber < period || (c.PeriodNumber == period && c.Id.CompareTo(id) < 0))
                : cycles.Where(c => c.PeriodNumber > period || (c.PeriodNumber == period && c.Id.CompareTo(id) > 0));
        }

        cycles = query.Descending
            ? cycles.OrderByDescending(c => c.PeriodNumber).ThenByDescending(c => c.Id)
            : cycles.OrderBy(c => c.PeriodNumber).ThenBy(c => c.Id);

        return Task.FromResult<IReadOnlyList<ContributionCycle>>(
            cycles.Take(query.PageSize + 1).ToList());
    }

}