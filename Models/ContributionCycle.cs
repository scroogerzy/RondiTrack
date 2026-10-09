namespace RondiTrack.Models;

/// <summary>
/// Represents a specific contribution period for a stokvel.
/// </summary>
public class ContributionCycle
{
    public Guid Id { get; private set; }

    public Guid StokvelId { get; private set; }

    public int PeriodNumber { get; private set; }

    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    public decimal TargetAmount { get; private set; }
// Navigation back to the parent stokvel.
public Stokvel Stokvel { get; private set; } = null!;
// A contribution cycle contains the contributions recorded for that period.
public ICollection<Contribution> Contributions { get; private set; }
    = new List<Contribution>();
    public ContributionCycle(
        Guid stokvelId,
        int periodNumber,
        DateTime startDate,
        DateTime endDate,
        decimal targetAmount)
    {
        if (stokvelId == Guid.Empty)
            throw new ArgumentException(
                "Stokvel ID is required.",
                nameof(stokvelId));

        if (periodNumber <= 0)
            throw new ArgumentException(
                "Period number must be greater than zero.",
                nameof(periodNumber));

        if (endDate < startDate)
            throw new ArgumentException(
                "End date must be on or after the start date.",
                nameof(endDate));

        if (targetAmount <= 0)
            throw new ArgumentException(
                "Target amount must be greater than zero.",
                nameof(targetAmount));

        Id = Guid.NewGuid();
        StokvelId = stokvelId;
        PeriodNumber = periodNumber;
        StartDate = startDate;
        EndDate = endDate;
        TargetAmount = targetAmount;
    }

    public void Update(
        int periodNumber,
        DateTime startDate,
        DateTime endDate,
        decimal targetAmount)
    {
        if (periodNumber <= 0)
            throw new ArgumentException(
                "Period number must be greater than zero.",
                nameof(periodNumber));

        if (endDate < startDate)
            throw new ArgumentException(
                "End date must be on or after the start date.",
                nameof(endDate));

        if (targetAmount <= 0)
            throw new ArgumentException(
                "Target amount must be greater than zero.",
                nameof(targetAmount));

        PeriodNumber = periodNumber;
        StartDate = startDate;
        EndDate = endDate;
        TargetAmount = targetAmount;
    }

    // EF Core uses this constructor when materializing
// an existing contribution cycle from PostgreSQL.
private ContributionCycle()
{
}
}