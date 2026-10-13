namespace RondiTrack.Domain.Models;

// A refresh token is stored by hash only. FamilyId links all tokens created
// by one login so reuse of a spent token revokes the entire session family.
public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid FamilyId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }
    public uint Version { get; private set; }

    private RefreshToken() { }

    private RefreshToken(Guid userId, Guid familyId, string tokenHash, DateTime now, TimeSpan lifetime)
    {
        if (userId == Guid.Empty) throw new ArgumentException("A refresh token must belong to a user.", nameof(userId));
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new ArgumentException("Token hash is required.", nameof(tokenHash));
        Id = Guid.NewGuid(); UserId = userId; FamilyId = familyId; TokenHash = tokenHash;
        CreatedAt = now; ExpiresAt = now.Add(lifetime);
    }

    public bool IsActive(DateTime now) => RevokedAt is null && now < ExpiresAt;

    public static RefreshToken StartFamily(Guid userId, string tokenHash, DateTime now, TimeSpan lifetime) =>
        new(userId, Guid.NewGuid(), tokenHash, now, lifetime);

    public RefreshToken Rotate(string newTokenHash, DateTime now, TimeSpan lifetime)
    {
        if (!IsActive(now)) throw new InvalidOperationException("Only an active refresh token can be rotated.");
        var successor = new RefreshToken(UserId, FamilyId, newTokenHash, now, lifetime);
        RevokedAt = now; ReplacedByTokenId = successor.Id;
        return successor;
    }
}
