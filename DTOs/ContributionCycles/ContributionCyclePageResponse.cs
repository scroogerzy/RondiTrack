namespace RondiTrack.DTOs.ContributionCycles;

/// <summary>
/// A bounded page of contribution cycles for one stokvel.
/// NextPageToken is empty when no further cycles are available.
/// </summary>
public sealed record ContributionCyclePageResponse(
    IReadOnlyList<ContributionCycleResponse> Items,
    string NextPageToken,
    int PageSize);
