namespace RondiTrack.DTOs.Contributions;

/// <summary>
/// A bounded page of contributions. NextPageToken is empty when no more
/// results exist for the query represented by this response.
/// </summary>
public sealed record ContributionPageResponse(
    IReadOnlyList<ContributionResponse> Items,
    string NextPageToken,
    int PageSize);
