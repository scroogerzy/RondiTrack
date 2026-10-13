using RondiTrack.Domain.Models;

namespace RondiTrack.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash);
    Task AddAsync(RefreshToken token);
    Task SaveRotationAsync(RefreshToken successor);
    Task RevokeFamilyAsync(Guid familyId, DateTime now);
}
