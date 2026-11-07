namespace RondiTrack.DTOs.Activity;

public sealed record ActivityFeedItemResponse(
    string Id,
    Guid StokvelId,
    string StokvelName,
    string Type,
    DateTime OccurredAtUtc,
    string Summary,
    Guid? RelatedUserId,
    string? RelatedUserName,
    long? AmountInCents,
    int? CycleNumber);

public sealed record ActivityFeedPageResponse(
    IReadOnlyList<ActivityFeedItemResponse> Items,
    string NextPageToken,
    int PageSize);
