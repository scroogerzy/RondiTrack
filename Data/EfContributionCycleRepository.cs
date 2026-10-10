using Microsoft.EntityFrameworkCore;
using RondiTrack.Models;

namespace RondiTrack.Data;

/// <summary>
/// EF Core repository for contribution cycle persistence.
/// </summary>
public class EfContributionCycleRepository
    : IContributionCycleRepository
{
    private readonly RondiTrackDbContext _dbContext;

    public EfContributionCycleRepository(
        RondiTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<ContributionCycle>>
        GetByStokvelAsync(Guid stokvelId)
    {
        return await _dbContext.ContributionCycles
            .Where(c => c.StokvelId == stokvelId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ContributionCycle?>
        GetByIdAsync(Guid id)
    {
        return await _dbContext.ContributionCycles
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<ContributionCycle?>
        GetByStokvelAndPeriodAsync(
            Guid stokvelId,
            int periodNumber)
    {
        return await _dbContext.ContributionCycles
            .FirstOrDefaultAsync(c =>
                c.StokvelId == stokvelId &&
                c.PeriodNumber == periodNumber);
    }

    public async Task<ContributionCycle>
        AddAsync(ContributionCycle cycle)
    {
        await _dbContext.ContributionCycles
            .AddAsync(cycle);

        await _dbContext.SaveChangesAsync();

        return cycle;
    }

    public async Task UpdateAsync(
        ContributionCycle cycle)
    {
        _dbContext.ContributionCycles
            .Update(cycle);

        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(
        Guid id)
    {
        var cycle = await GetByIdAsync(id);

        if (cycle is null)
            return false;

        _dbContext.ContributionCycles
            .Remove(cycle);

        await _dbContext.SaveChangesAsync();

        return true;
    }
    public async Task<IReadOnlyList<ContributionCycle>> GetPageByStokvelAsync(
        Guid stokvelId,
        ContributionCyclePageQuery query)
    {
        IQueryable<ContributionCycle> cycles = _dbContext.ContributionCycles
            .AsNoTracking()
            .Where(cycle => cycle.StokvelId == stokvelId);

        if (query.LastPeriodNumber.HasValue && query.LastId.HasValue)
        {
            var lastPeriodNumber = query.LastPeriodNumber.Value;
            var lastId = query.LastId.Value;
            cycles = query.Descending
                ? cycles.Where(cycle =>
                    cycle.PeriodNumber < lastPeriodNumber ||
                    (cycle.PeriodNumber == lastPeriodNumber &&
                     cycle.Id.CompareTo(lastId) < 0))
                : cycles.Where(cycle =>
                    cycle.PeriodNumber > lastPeriodNumber ||
                    (cycle.PeriodNumber == lastPeriodNumber &&
                     cycle.Id.CompareTo(lastId) > 0));
        }

        cycles = query.Descending
            ? cycles.OrderByDescending(cycle => cycle.PeriodNumber)
                .ThenByDescending(cycle => cycle.Id)
            : cycles.OrderBy(cycle => cycle.PeriodNumber)
                .ThenBy(cycle => cycle.Id);

        return await cycles.Take(query.PageSize + 1).ToListAsync();
    }

}