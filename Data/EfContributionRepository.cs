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
}