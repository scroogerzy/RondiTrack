namespace RondiTrack.DTOs.ContributionCycles;

public record CreateContributionCycleRequest(
    int PeriodNumber,
    DateTime StartDate,
    DateTime EndDate,
    decimal TargetAmount);