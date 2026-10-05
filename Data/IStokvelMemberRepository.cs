using RondiTrack.Models;

namespace RondiTrack.Data;

// StokvelMember has a composite key, so its data-access contract
// deliberately uses UserId and StokvelId rather than a single Guid Id.
public interface IStokvelMemberRepository
{
    Task<IEnumerable<StokvelMember>> GetByStokvelIdAsync(
        Guid stokvelId);

    Task<StokvelMember?> GetAsync(
        Guid userId,
        Guid stokvelId);

    Task<StokvelMember> AddAsync(
        StokvelMember member);

    Task<bool> DeleteAsync(
        Guid userId,
        Guid stokvelId);
}