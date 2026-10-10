namespace RondiTrack.Data;

public sealed record ContributionCyclePageQuery(
    int PageSize,
    bool Descending,
    int? LastPeriodNumber,
    Guid? LastId);
