namespace RondiTrack.Domain.Models;

public sealed class StokvelInvite
{
    public Guid Id { get; private set; }
    public Guid StokvelId { get; private set; }
    public string CodeHash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    private StokvelInvite() { }
    public StokvelInvite(Guid stokvelId, string codeHash, DateTime expiresAtUtc, Guid createdByUserId, DateTime createdAtUtc)
    { Id = Guid.NewGuid(); StokvelId = stokvelId; CodeHash = codeHash; ExpiresAtUtc = expiresAtUtc; CreatedByUserId = createdByUserId; CreatedAtUtc = createdAtUtc; }
}
