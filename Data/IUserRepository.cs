using RondiTrack.Models;

namespace RondiTrack.Data;

// Defines the data-access operations required for Users.
// The controller depends on this abstraction rather than a concrete list.
public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();

    Task<User?> GetByIdAsync(Guid id);

    Task<User> AddAsync(User user);

    Task<bool> UpdateAsync(User user);

    Task<bool> DeleteAsync(Guid id);
}