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
}