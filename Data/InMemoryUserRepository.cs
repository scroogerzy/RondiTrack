using RondiTrack.Models;

namespace RondiTrack.Data;

// In-memory implementation of the User repository.
// A Singleton registration keeps the seeded data available
// for the lifetime of the running application.
public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public InMemoryUserRepository()
    {
        // Seed initial users so the API has data immediately.
        _users.Add(new User(
            "Thabo Mokoena",
            "thabo@example.com"));

        _users.Add(new User(
            "Lerato Dlamini",
            "lerato@example.com"));

        _users.Add(new User(
            "Sibusiso Ndlovu",
            "sibusiso@example.com"));
    }

    public Task<IEnumerable<User>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<User>>(_users);
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);

        return Task.FromResult(user);
    }

    public Task<User> AddAsync(User user)
    {
        _users.Add(user);

        return Task.FromResult(user);
    }

    public Task<bool> UpdateAsync(User user)
    {
        var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);

        if (existingUser is null)
            return Task.FromResult(false);

        // The entity itself controls valid property changes.
        existingUser.UpdateFullName(user.FullName);
        existingUser.UpdateEmail(user.Email);

        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);

        if (user is null)
            return Task.FromResult(false);

        _users.Remove(user);

        return Task.FromResult(true);
    }
}