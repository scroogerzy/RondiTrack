using RondiTrack.Models;

namespace RondiTrack.Data;

public class InMemoryContributionCycleRepository
    : IContributionCycleRepository
{
    private readonly List<ContributionCycle> _cycles = new();

    public Task<IEnumerable<ContributionCycle>> GetByStokvelAsync(
        Guid stokvelId)
    {
        var cycles = _cycles
            .Where(c => c.StokvelId == stokvelId)
            .OrderBy(c => c.PeriodNumber)
            .ToList();

        return Task.FromResult<IEnumerable<ContributionCycle>>(cycles);
    }

    public Task<ContributionCycle?> GetByIdAsync(
        Guid id)
    {
        var cycle = _cycles.FirstOrDefault(c => c.Id == id);

        return Task.FromResult(cycle);
    }

    public Task<ContributionCycle?> GetByStokvelAndPeriodAsync(
        Guid stokvelId,
        int periodNumber)
    {
        var cycle = _cycles.FirstOrDefault(c =>
            c.StokvelId == stokvelId &&
            c.PeriodNumber == periodNumber);

        return Task.FromResult(cycle);
    }

    public Task<ContributionCycle> AddAsync(
        ContributionCycle cycle)
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
        var cycle = _cycles.FirstOrDefault(c => c.Id == id);

        if (cycle is null)
            return Task.FromResult(false);

        _cycles.Remove(cycle);

        return Task.FromResult(true);
    }
}