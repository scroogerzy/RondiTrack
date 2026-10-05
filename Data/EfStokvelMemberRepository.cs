using Microsoft.EntityFrameworkCore;
using RondiTrack.Models;

namespace RondiTrack.Data;

// PostgreSQL-backed repository for membership data.
// Its operations reflect StokvelMember's composite-key identity.
public class EfStokvelMemberRepository
    : IStokvelMemberRepository
{
    private readonly RondiTrackDbContext _dbContext;

    public EfStokvelMemberRepository(
        RondiTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<StokvelMember>>
        GetByStokvelIdAsync(Guid stokvelId)
    {
        // This path is read-only, so change tracking is unnecessary.
        return await _dbContext.StokvelMembers
            .AsNoTracking()
            .Where(member => member.StokvelId == stokvelId)
            .ToListAsync();
    }

    public async Task<StokvelMember?> GetAsync(
        Guid userId,
        Guid stokvelId)
    {
        // FindAsync accepts both values because the entity
        // uses a composite primary key.
        return await _dbContext.StokvelMembers.FindAsync(
            userId,
            stokvelId);
    }

    public async Task<StokvelMember> AddAsync(
        StokvelMember member)
    {
        await _dbContext.StokvelMembers.AddAsync(member);
        await _dbContext.SaveChangesAsync();

        return member;
    }

    public async Task<bool> DeleteAsync(
        Guid userId,
        Guid stokvelId)
    {
        var member = await _dbContext.StokvelMembers.FindAsync(
            userId,
            stokvelId);

        if (member is null)
            return false;

        _dbContext.StokvelMembers.Remove(member);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}