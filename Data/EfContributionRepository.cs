using Microsoft.EntityFrameworkCore;
using RondiTrack.Models;

namespace RondiTrack.Data;

// EF Core repository for database-backed contribution reads and writes.
// Keeping this separate from the in-memory repository allows the API to
// measure real PostgreSQL query behaviour for Assignment 5.2.
public class EfContributionRepository : IContributionRepository
{
    private readonly RondiTrackDbContext _dbContext;

    public EfContributionRepository(RondiTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Contribution?> GetByMemberAndCycleAsync(
        Guid stokvelId,
        Guid userId,
        int cycle)
    {
        return await _dbContext.Contributions
            .FirstOrDefaultAsync(contribution =>
                contribution.StokvelId == stokvelId &&
                contribution.UserId == userId &&
                contribution.Cycle == cycle);
    }

    public async Task<Contribution> AddAsync(
        Contribution contribution)
    {
        await _dbContext.Contributions.AddAsync(contribution);
        await _dbContext.SaveChangesAsync();

        return contribution;
    }

    public async Task<IEnumerable<Contribution>> GetByStokvelAsync(
        Guid stokvelId)
    {
        return await _dbContext.Contributions
            .Where(contribution => contribution.StokvelId == stokvelId)
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<IReadOnlyList<Contribution>> GetPageByCycleAsync(
        Guid stokvelId,
        int cycleNumber,
        ContributionPageQuery query)
    {
        IQueryable<Contribution> contributions = _dbContext.Contributions
            .AsNoTracking()
            .Where(contribution =>
                contribution.StokvelId == stokvelId &&
                contribution.Cycle == cycleNumber);

        if (query.UserId.HasValue)
        {
            contributions = contributions.Where(contribution =>
                contribution.UserId == query.UserId.Value);
        }

        if (query.LastRecordedAt.HasValue && query.LastId.HasValue)
        {
            var lastRecordedAt = query.LastRecordedAt.Value;
            var lastId = query.LastId.Value;
            contributions = query.Descending
                ? contributions.Where(contribution =>
                    contribution.RecordedAt < lastRecordedAt ||
                    (contribution.RecordedAt == lastRecordedAt &&
                     contribution.Id.CompareTo(lastId) < 0))
                : contributions.Where(contribution =>
                    contribution.RecordedAt > lastRecordedAt ||
                    (contribution.RecordedAt == lastRecordedAt &&
                     contribution.Id.CompareTo(lastId) > 0));
        }

        contributions = query.Descending
            ? contributions.OrderByDescending(contribution => contribution.RecordedAt)
                .ThenByDescending(contribution => contribution.Id)
            : contributions.OrderBy(contribution => contribution.RecordedAt)
                .ThenBy(contribution => contribution.Id);

        return await contributions
            .Take(query.PageSize + 1)
            .ToListAsync();
    }

}