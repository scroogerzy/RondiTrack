using Microsoft.EntityFrameworkCore;
using RondiTrack.Domain.Models;
using RondiTrack.Domain.Repositories;

namespace RondiTrack.Infrastructure.Data;

public sealed class EfRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly RondiTrackDbContext _dbContext;
    public EfRefreshTokenRepository(RondiTrackDbContext dbContext) => _dbContext = dbContext;

    public Task<RefreshToken?> GetByHashAsync(string tokenHash) =>
        _dbContext.RefreshTokens.FirstOrDefaultAsync(token => token.TokenHash == tokenHash);

    public async Task AddAsync(RefreshToken token)
    {
        _dbContext.RefreshTokens.Add(token);
        await _dbContext.SaveChangesAsync();
    }

    public async Task SaveRotationAsync(RefreshToken successor)
    {
        _dbContext.RefreshTokens.Add(successor);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RevokeFamilyAsync(Guid familyId, DateTime now) =>
        await _dbContext.RefreshTokens
            .Where(token => token.FamilyId == familyId && token.RevokedAt == null)
            .ExecuteUpdateAsync(update => update.SetProperty(token => token.RevokedAt, (DateTime?)now));
}
