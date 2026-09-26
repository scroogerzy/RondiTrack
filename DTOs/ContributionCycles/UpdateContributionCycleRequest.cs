namespace RondiTrack.DTOs.ContributionCycles;

public record UpdateContributionCycleRequest(
    int PeriodNumber,
    DateTime StartDate,
    DateTime EndDate,
    decimal TargetAmount);