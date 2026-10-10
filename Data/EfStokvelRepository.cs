
using Microsoft.EntityFrameworkCore;
using RondiTrack.Models;

namespace RondiTrack.Data;

/// <summary>
/// PostgreSQL-backed implementation of IStokvelRepository.
/// Stokvel records must be persisted in the same database as
/// their contribution cycles and contributions.
/// </summary>
public class EfStokvelRepository : IStokvelRepository
{
    private readonly RondiTrackDbContext _dbContext;

    public EfStokvelRepository(RondiTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Retrieves all stokvels from PostgreSQL.
    /// This is a read-only operation, so change tracking is unnecessary.
    /// </summary>
    public async Task<IEnumerable<Stokvel>> GetAllAsync()
    {
        return await _dbContext.Stokvels
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a stokvel by its primary key.
    /// </summary>
    public async Task<Stokvel?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Stokvels.FindAsync(id);
    }

    /// <summary>
    /// Inserts a new stokvel into PostgreSQL.
    /// The entity performs its own business validation.
    /// </summary>
    public async Task<Stokvel> AddAsync(Stokvel stokvel)
    {
        await _dbContext.Stokvels.AddAsync(stokvel);
        await _dbContext.SaveChangesAsync();

        return stokvel;
    }

    /// <summary>
    /// Updates an existing persisted stokvel.
    /// The existing tracked entity remains responsible for applying
    /// validated changes through its domain methods.
    /// </summary>
    public async Task<bool> UpdateAsync(Stokvel stokvel)
    {
        var existingStokvel =
            await _dbContext.Stokvels.FindAsync(stokvel.Id);

        if (existingStokvel is null)
        {
            return false;
        }

        // Use domain methods instead of assigning private-set properties.
        existingStokvel.UpdateName(stokvel.Name);
        existingStokvel.UpdateContribution(
            stokvel.MonthlyContribution);

        await _dbContext.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Deletes a stokvel that exists in PostgreSQL.
    /// Database foreign-key rules continue to apply.
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var stokvel =
            await _dbContext.Stokvels.FindAsync(id);

        if (stokvel is null)
        {
            return false;
        }

        _dbContext.Stokvels.Remove(stokvel);

        await _dbContext.SaveChangesAsync();

        return true;
    }
}
