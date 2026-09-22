namespace RondiTrack.Models;

// A stokvel represents a savings group.
// The entity protects its own business rules.
public class Stokvel
{
    // Internal collection hidden from callers.
    // External code must use AddMember() and RemoveMember().
    private readonly List<Guid> _memberIds = new();

    public Guid Id { get; }

    public string Name { get; private set; }

    // Decimal is the correct choice for money because it avoids
    // floating-point precision issues.
    public decimal MonthlyContribution { get; private set; }

    // Expose a read-only view of members.
    public IReadOnlyCollection<Guid> MemberIds => _memberIds.AsReadOnly();

    public Stokvel(string name, decimal monthlyContribution)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Stokvel name is required.",
                nameof(name));

        // The stokvel contribution is strictly R500.
        // Values below or above R500 are invalid.
        if (monthlyContribution != 500m)
            throw new ArgumentException(
                "Monthly contribution must be exactly R500.",
                nameof(monthlyContribution));

        Id = Guid.NewGuid();
        Name = name;
        MonthlyContribution = monthlyContribution;
    }

    // Membership rules belong inside the entity.
    public void AddMember(Guid userId)
    {
        if (_memberIds.Contains(userId))
            throw new InvalidOperationException(
                "User is already a member of this stokvel.");

        if (_memberIds.Count >= 20)
            throw new InvalidOperationException(
                "A stokvel may not exceed 20 members.");

        _memberIds.Add(userId);
    }

    public void RemoveMember(Guid userId)
    {
        if (!_memberIds.Contains(userId))
            throw new InvalidOperationException(
                "User is not a member of this stokvel.");

        _memberIds.Remove(userId);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Stokvel name is required.",
                nameof(name));

        Name = name;
    }

    public void UpdateContribution(decimal monthlyContribution)
    {
        // The contribution must remain exactly R500.
        if (monthlyContribution != 500m)
            throw new ArgumentException(
                "Monthly contribution must be exactly R500.",
                nameof(monthlyContribution));

        MonthlyContribution = monthlyContribution;
    }
}