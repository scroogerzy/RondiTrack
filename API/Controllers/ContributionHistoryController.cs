using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RondiTrack.Auth;
using RondiTrack.Common;
using RondiTrack.DTOs.Contributions;
using RondiTrack.Infrastructure.Data;
using RondiTrack.Mappings;

namespace RondiTrack.Controllers;

/// <summary>Stokvel-wide contribution history, filtered and keyset-paginated.</summary>
[ApiController]
[Route(ApiRoutes.Stokvels + "/{stokvelId:guid}/contributions")]
public sealed class ContributionHistoryController(RondiTrackDbContext db) : ControllerBase
{
    [StokvelAuthorize(Policies.StokvelMember)]
    [HttpGet]
    public async Task<ActionResult<ContributionPageResponse>> Get(
        Guid stokvelId,
        [FromQuery] int? pageSize,
        [FromQuery] string? pageToken,
        [FromQuery] Guid? userId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] string sortDirection = "desc",
        CancellationToken cancellationToken = default)
    {
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "pageSize", "pageToken", "userId", "from", "to", "sortDirection"
        };
        var unknown = Request.Query.Keys.FirstOrDefault(key => !allowed.Contains(key));
        if (unknown is not null)
            return ProblemResponses.BadRequest($"Unsupported query parameter '{unknown}'.", Request.Path);

        if (pageSize < 0)
            return ProblemResponses.BadRequest("pageSize cannot be negative.", Request.Path);

        var size = pageSize.GetValueOrDefault(20);
        if (size == 0) size = 20;
        size = Math.Min(size, 100);

        sortDirection = sortDirection.Trim().ToLowerInvariant();
        if (sortDirection is not ("asc" or "desc"))
            return ProblemResponses.BadRequest("sortDirection must be either 'asc' or 'desc'.", Request.Path);

        // The API contract accepts UTC instants: from is inclusive and to is exclusive.
        var fromUtc = from?.UtcDateTime;
        var toUtc = to?.UtcDateTime;
        if (fromUtc.HasValue && toUtc.HasValue && fromUtc.Value >= toUtc.Value)
            return ProblemResponses.BadRequest("The 'to' instant must be later than the 'from' instant.", Request.Path);

        if (userId.HasValue && !await db.StokvelMembers.AsNoTracking().AnyAsync(
            member => member.StokvelId == stokvelId && member.UserId == userId.Value,
            cancellationToken))
            return ProblemResponses.BadRequest("The selected member does not belong to this stokvel.", Request.Path);

        var descending = sortDirection == "desc";
        DateTime? lastRecordedAt = null;
        Guid? lastId = null;
        if (!string.IsNullOrWhiteSpace(pageToken))
        {
            if (!ContributionHistoryPageToken.TryDecode(pageToken, out var token) || token is null)
                return ProblemResponses.BadRequest("pageToken is malformed or unsupported.", Request.Path);

            if (token.StokvelId != stokvelId || token.UserId != userId ||
                token.FromUtc != fromUtc || token.ToUtc != toUtc ||
                token.SortBy != "recordedAt" || token.Descending != descending)
                return ProblemResponses.BadRequest(
                    "pageToken cannot be reused with a different member, date range or sort order.", Request.Path);

            lastRecordedAt = token.LastRecordedAtUtc;
            lastId = token.LastId;
        }

        var query = db.Contributions
            .AsNoTracking()
            .Include(contribution => contribution.ContributionCycle)
            .Where(contribution => contribution.StokvelId == stokvelId);

        if (userId.HasValue)
            query = query.Where(contribution => contribution.UserId == userId.Value);
        if (fromUtc.HasValue)
            query = query.Where(contribution => contribution.RecordedAt >= fromUtc.Value);
        if (toUtc.HasValue)
            query = query.Where(contribution => contribution.RecordedAt < toUtc.Value);

        if (lastRecordedAt.HasValue && lastId.HasValue)
        {
            var cursorTime = lastRecordedAt.Value;
            var cursorId = lastId.Value;
            query = descending
                ? query.Where(contribution => contribution.RecordedAt < cursorTime ||
                    (contribution.RecordedAt == cursorTime && contribution.Id.CompareTo(cursorId) < 0))
                : query.Where(contribution => contribution.RecordedAt > cursorTime ||
                    (contribution.RecordedAt == cursorTime && contribution.Id.CompareTo(cursorId) > 0));
        }

        query = descending
            ? query.OrderByDescending(contribution => contribution.RecordedAt)
                .ThenByDescending(contribution => contribution.Id)
            : query.OrderBy(contribution => contribution.RecordedAt)
                .ThenBy(contribution => contribution.Id);

        var fetched = await query.Take(size + 1).ToListAsync(cancellationToken);
        var hasMore = fetched.Count > size;
        var items = fetched.Take(size).ToList();
        var next = string.Empty;
        if (hasMore && items.Count > 0)
        {
            var last = items[^1];
            next = new ContributionHistoryPageToken(
                1, stokvelId, userId, fromUtc, toUtc, "recordedAt", descending,
                last.RecordedAt, last.Id).Encode();
        }

        return Ok(new ContributionPageResponse(
            items.Select(item => item.ToResponse()).ToList(), next, size));
    }
}
