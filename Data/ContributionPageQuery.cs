namespace RondiTrack.Data;

/// <summary>
/// Database query options for the paged contributions endpoint.
/// Cursor values are supplied only after the opaque page token is validated.
/// </summary>
public sealed record ContributionPageQuery(
    int PageSize,
    Guid? UserId,
    bool Descending,
    DateTime? LastRecordedAt,
    Guid? LastId);
