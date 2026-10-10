namespace RondiTrack.Models;

/// <summary>
/// A stokvel represents a savings group.
/// The entity protects its own business rules.
/// </summary>
public class Stokvel
{
    // Internal collection hidden from callers.
    // External code must use AddMember() and RemoveMember().
    private readonly List<Guid> _memberIds = new();

    public Guid Id { get; private set; }
    public uint Version { get; private set; }

    public string Name { get; private set; } = null!;

    // Decimal is the correct choice for money because it avoids
    // floating-point precision issues.
    public decimal MonthlyContribution { get; private set; }

    // Expose a read-only view of members.
    public IReadOnlyCollection<Guid> MemberIds => _memberIds.AsReadOnly();

    // A Stokvel has many memberships.
    // Membership-specific information such as Role and JoinedAtUtc
    // belongs to StokvelMember rather than directly to User or Stokvel.
    public ICollection<StokvelMember> Members { get; private set; }
        = new List<StokvelMember>();

    // A stokvel can contain many contribution cycles.
    public ICollection<ContributionCycle> ContributionCycles { get; private set; }
        = new List<ContributionCycle>();

    public Stokvel(
        string name,
        decimal monthlyContribution)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Stokvel name is required.",
                nameof(name));
        }

        // The stokvel contribution is strictly R500.
        if (monthlyContribution != 500m)
        {
            throw new ArgumentException(
                "Monthly contribution must be exactly R500.",
                nameof(monthlyContribution));
        }

        Id = Guid.NewGuid();
        Name = name;
        MonthlyContribution = monthlyContribution;
    }

    // Membership rules belong inside the entity.
    public void AddMember(Guid userId)
    {
        if (_memberIds.Contains(userId))
        {
            throw new InvalidOperationException(
                "User is already a member of this stokvel.");
        }

        if (_memberIds.Count >= 20)
        {
            throw new InvalidOperationException(
                "A stokvel may not exceed 20 members.");
        }

        _memberIds.Add(userId);
    }

    public void RemoveMember(Guid userId)
    {
        if (!_memberIds.Contains(userId))
        {
            throw new InvalidOperationException(
                "User is not a member of this stokvel.");
        }

        _memberIds.Remove(userId);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Stokvel name is required.",
                nameof(name));
        }

        Name = name;
    }

    public void UpdateContribution(decimal monthlyContribution)
    {
        if (monthlyContribution != 500m)
        {
            throw new ArgumentException(
                "Monthly contribution must be exactly R500.",
                nameof(monthlyContribution));
        }

        MonthlyContribution = monthlyContribution;
    }

    // EF Core uses this constructor when materializing
    // an existing stokvel from PostgreSQL.
    private Stokvel()
    {
    }
}