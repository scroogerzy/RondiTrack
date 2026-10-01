using Microsoft.EntityFrameworkCore;
using RondiTrack.Models;

namespace RondiTrack.Data;

// EF Core implementation of the User repository.
// The interface remains unchanged so controllers and services
// do not need to know whether Users are stored in memory or PostgreSQL.
public class EfUserRepository : IUserRepository
{
    private readonly RondiTrackDbContext _dbContext;

    public EfUserRepository(RondiTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        // AsNoTracking is appropriate for read-only queries because
        // the returned entities are not being edited in this operation.
        return await _dbContext.Users
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        // FindAsync uses the configured primary key to retrieve one User.
        return await _dbContext.Users.FindAsync(id);
    }

    public async Task<User> AddAsync(User user)
    {
        // The entity has already passed its domain validation.
        // EF now tracks it and persists it when SaveChangesAsync runs.
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        return user;
    }

    public async Task<bool> UpdateAsync(User user)
    {
        // Retrieve the existing database row first so the update operates
        // on an entity tracked by this DbContext.
        var existingUser = await _dbContext.Users.FindAsync(user.Id);

        if (existingUser is null)
            return false;

        // The domain entity remains responsible for validating changes.
        existingUser.UpdateFullName(user.FullName);
        existingUser.UpdateEmail(user.Email);

        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        // Find the existing row before attempting deletion.
        var user = await _dbContext.Users.FindAsync(id);

        if (user is null)
            return false;

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}