using RondiTrack.Models;

namespace RondiTrack.Data;

// In-memory implementation of the Stokvel repository.
// This will later be replaced by EF Core and PostgreSQL.
public class InMemoryStokvelRepository : IStokvelRepository
{
    private readonly List<Stokvel> _stokvels = new();

    public InMemoryStokvelRepository()
    {
        // Seed initial stokvels so the API has usable data immediately.
        // The domain rule requires every contribution to be exactly R500.

        var stokvel1 = new Stokvel(
            "Ubuntu Savings Club",
            500m);

        var stokvel2 = new Stokvel(
            "Mzanzi Monthly Stokvel",
            500m);

        _stokvels.Add(stokvel1);
        _stokvels.Add(stokvel2);
    }

    public Task<IEnumerable<Stokvel>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Stokvel>>(_stokvels);
    }

    public Task<Stokvel?> GetByIdAsync(Guid id)
    {
        var stokvel = _stokvels.FirstOrDefault(s => s.Id == id);

        return Task.FromResult(stokvel);
    }

    public Task<Stokvel> AddAsync(Stokvel stokvel)
    {
        _stokvels.Add(stokvel);

        return Task.FromResult(stokvel);
    }

    public Task<bool> UpdateAsync(Stokvel stokvel)
    {
        var existingStokvel = _stokvels.FirstOrDefault(s => s.Id == stokvel.Id);

        if (existingStokvel is null)
            return Task.FromResult(false);

        // The entity controls its own validation rules.
        existingStokvel.UpdateName(stokvel.Name);
        existingStokvel.UpdateContribution(stokvel.MonthlyContribution);

        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var stokvel = _stokvels.FirstOrDefault(s => s.Id == id);

        if (stokvel is null)
            return Task.FromResult(false);

        _stokvels.Remove(stokvel);

        return Task.FromResult(true);
    }
}