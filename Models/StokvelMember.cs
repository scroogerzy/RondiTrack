namespace RondiTrack.Models;

// StokvelMember is an explicit join entity between User and Stokvel.
// The relationship carries its own domain data, so it cannot be modeled
// as an implicit many-to-many join table.
public class StokvelMember
{
    public Guid UserId { get; private set; }

    public Guid StokvelId { get; private set; }

    public string Role { get; private set; } = null!;

    public DateTime JoinedAtUtc { get; private set; }

    // Navigation properties allow EF Core to understand that this
    // membership belongs to one User and one Stokvel.
    public User User { get; private set; } = null!;

    public Stokvel Stokvel { get; private set; } = null!;

    // EF Core uses this constructor when loading persisted memberships.
    private StokvelMember()
    {
    }

    public StokvelMember(
        Guid userId,
        Guid stokvelId,
        string role,
        DateTime joinedAtUtc)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User id is required.",
                nameof(userId));

        if (stokvelId == Guid.Empty)
            throw new ArgumentException(
                "Stokvel id is required.",
                nameof(stokvelId));

        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException(
                "Member role is required.",
                nameof(role));

        UserId = userId;
        StokvelId = stokvelId;
        Role = role;
        JoinedAtUtc = joinedAtUtc;
    }

    // Membership role changes remain controlled by the entity
    // rather than exposing a public property setter.
    public void UpdateRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException(
                "Member role is required.",
                nameof(role));

        Role = role;
    }
}