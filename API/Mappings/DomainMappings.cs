using RondiTrack.DTOs.Contributions;
using RondiTrack.DTOs.ContributionCycles;
using RondiTrack.DTOs.Stokvels;
using RondiTrack.DTOs.Users;
using RondiTrack.Domain.Models;

namespace RondiTrack.Mappings;

/// <summary>
/// Provides explicit mappings between domain entities and response DTOs.
/// </summary>
public static class DomainMappings
{
    /// <summary>
    /// Maps a User entity to a UserResponse DTO.
    /// </summary>
    public static UserResponse ToResponse(this User user)
    {
        return new UserResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Version);
    }

    /// <summary>
    /// Maps a Stokvel entity to a StokvelResponse DTO.
    /// </summary>
    public static StokvelResponse ToResponse(
        this Stokvel stokvel)
    {
        return new StokvelResponse(
            stokvel.Id,
            stokvel.Name,
            stokvel.MonthlyContribution,
            stokvel.MemberIds.Count,
            stokvel.Version);
    }

    /// <summary>
    /// Maps a Contribution entity to a ContributionResponse DTO.
    /// </summary>
    public static ContributionResponse ToResponse(
        this Contribution contribution,
        Guid? cycleId = null)
    {
        var stableCycleId = cycleId ?? contribution.ContributionCycle?.Id;
        if (!stableCycleId.HasValue || stableCycleId.Value == Guid.Empty)
        {
            throw new InvalidOperationException(
                "The contribution cycle must be loaded or supplied when mapping a contribution response.");
        }

        return new ContributionResponse(
            contribution.Id,
            contribution.StokvelId,
            contribution.UserId,
            contribution.Cycle,
            stableCycleId.Value,
            contribution.Amount,
            contribution.RecordedAt);
    }

    public static ContributionCycleResponse ToResponse(
        this ContributionCycle cycle)
    {
        var collected = cycle.Contributions.Sum(contribution => contribution.Amount);
        return new ContributionCycleResponse(
            cycle.Id,
            cycle.StokvelId,
            cycle.PeriodNumber,
            cycle.StartDate,
            cycle.EndDate,
            cycle.TargetAmount,
            cycle.Version,
            cycle.PayoutProcessedAtUtc,
            cycle.ClosedAtUtc,
            cycle.PayoutProcessedAtUtc is not null
                ? "PaidOut"
                : cycle.ClosedAtUtc is not null
                    ? "Closed"
                    : "Open",
            ToCents(collected),
            ToCents(cycle.TargetAmount));
    }

    private static long ToCents(decimal amount) =>
        decimal.ToInt64(decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero));
}
