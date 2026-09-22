namespace RondiTrack.Models;

/// <summary>
/// Represents a contribution made by a user to a stokvel for a specific cycle.
/// </summary>
public class Contribution
{
    /// <summary>
    /// Gets the unique identifier of the contribution.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the stokvel receiving the contribution.
    /// </summary>
    public Guid StokvelId { get; }

    /// <summary>
    /// Gets the user who made the contribution.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Gets the contribution cycle number.
    /// </summary>
    public int Cycle { get; }

    /// <summary>
    /// Gets the amount contributed.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the UTC date and time when the contribution was recorded.
    /// </summary>
    public DateTime RecordedAt { get; }

    /// <summary>
    /// Creates a new contribution.
    /// </summary>
    public Contribution(
        Guid stokvelId,
        Guid userId,
        int cycle,
        decimal amount)
    {
        if (stokvelId == Guid.Empty)
            throw new ArgumentException(
                "Stokvel ID is required.",
                nameof(stokvelId));

        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));

        if (cycle <= 0)
            throw new ArgumentException(
                "Contribution cycle must be greater than zero.",
                nameof(cycle));

        if (amount != 500m)
            throw new ArgumentException(
                "Contribution amount must be exactly R500.",
                nameof(amount));

        Id = Guid.NewGuid();
        StokvelId = stokvelId;
        UserId = userId;
        Cycle = cycle;
        Amount = amount;
        RecordedAt = DateTime.UtcNow;
    }
}