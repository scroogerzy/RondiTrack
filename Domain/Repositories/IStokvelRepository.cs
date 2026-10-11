using RondiTrack.Domain.Models;

namespace RondiTrack.Domain.Repositories;

// Defines the data-access operations required for Stokvels.
public interface IStokvelRepository
{
    Task<IEnumerable<Stokvel>> GetAllAsync();

    Task<Stokvel?> GetByIdAsync(Guid id);

    Task<Stokvel> AddAsync(Stokvel stokvel);

    Task<bool> UpdateAsync(Stokvel stokvel);

    Task<bool> DeleteAsync(Guid id);
}