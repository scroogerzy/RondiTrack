using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RondiTrack.Common;
using RondiTrack.DTOs.Activity;
using RondiTrack.Infrastructure.Data;

namespace RondiTrack.Controllers;

/// <summary>
/// Recent activity across only the stokvels to which the caller currently belongs.
/// Activity is projected from persisted contribution, payout and membership records.
/// </summary>
[ApiController]
[Authorize]
[Route(ApiRoutes.V1 + "/activity")]
public sealed class ActivityController(RondiTrackDbContext db) : ControllerBase
{
    private sealed record ActivityRow(
        Guid EntityId,
        Guid StokvelId,
        string StokvelName,
        string Type,
        int TypeOrder,
        DateTime OccurredAtUtc,
        Guid? RelatedUserId,
        string? RelatedUserName,
        decimal? Amount,
        int? CycleNumber);

    [HttpGet]
    public async Task<ActionResult<ActivityFeedPageResponse>> Get(
        [FromQuery] int? pageSize,
        [FromQuery] string? pageToken,
        CancellationToken cancellationToken = default)
    {
        var unknown = Request.Query.Keys.FirstOrDefault(key =>
            !string.Equals(key, "pageSize", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(key, "pageToken", StringComparison.OrdinalIgnoreCase));
        if (unknown is not null)
            return ProblemResponses.BadRequest($"Unsupported query parameter '{unknown}'.", Request.Path);
        if (pageSize < 0)
            return ProblemResponses.BadRequest("pageSize cannot be negative.", Request.Path);

        var userClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userClaim, out var userId))
            return Unauthorized();

        var size = pageSize.GetValueOrDefault(20);
        if (size == 0) size = 20;
        size = Math.Min(size, 50);

        ActivityFeedPageToken? cursor = null;
        if (!string.IsNullOrWhiteSpace(pageToken))
        {
            if (!ActivityFeedPageToken.TryDecode(pageToken, out cursor) || cursor is null)
                return ProblemResponses.BadRequest("pageToken is malformed or unsupported.", Request.Path);
            if (cursor.UserId != userId)
                return ProblemResponses.BadRequest("pageToken belongs to a different member.", Request.Path);
        }

        var stokvelIds = await db.StokvelMembers.AsNoTracking()
            .Where(member => member.UserId == userId)
            .Select(member => member.StokvelId)
            .ToListAsync(cancellationToken);

        if (stokvelIds.Count == 0)
            return Ok(new ActivityFeedPageResponse(Array.Empty<ActivityFeedItemResponse>(), string.Empty, size));

        var contributions =
            from contribution in db.Contributions.AsNoTracking()
            join stokvel in db.Stokvels.AsNoTracking() on contribution.StokvelId equals stokvel.Id
            join member in db.Users.AsNoTracking() on contribution.UserId equals member.Id
            where stokvelIds.Contains(contribution.StokvelId)
            select new { Contribution = contribution, StokvelName = stokvel.Name, MemberName = member.FullName };

        if (cursor is not null)
        {
            var time = cursor.LastOccurredAtUtc;
            if (3 < cursor.LastTypeOrder)
                contributions = contributions.Where(x => x.Contribution.RecordedAt <= time);
            else if (3 > cursor.LastTypeOrder)
                contributions = contributions.Where(x => x.Contribution.RecordedAt < time);
            else
            {
                var stokvelId = cursor.LastStokvelId;
                var entityId = cursor.LastEntityId;
                contributions = contributions.Where(x =>
                    x.Contribution.RecordedAt < time ||
                    (x.Contribution.RecordedAt == time &&
                     (x.Contribution.StokvelId.CompareTo(stokvelId) < 0 ||
                      (x.Contribution.StokvelId == stokvelId &&
                       x.Contribution.Id.CompareTo(entityId) < 0))));
            }
        }

        var contributionRows = await contributions
            .OrderByDescending(x => x.Contribution.RecordedAt)
            .ThenByDescending(x => x.Contribution.StokvelId)
            .ThenByDescending(x => x.Contribution.Id)
            .Take(size + 1)
            .Select(x => new ActivityRow(
                x.Contribution.Id, x.Contribution.StokvelId, x.StokvelName,
                "Contribution", 3, x.Contribution.RecordedAt,
                x.Contribution.UserId, x.MemberName,
                x.Contribution.Amount, x.Contribution.Cycle))
            .ToListAsync(cancellationToken);

        var payouts =
            from cycle in db.ContributionCycles.AsNoTracking()
            join stokvel in db.Stokvels.AsNoTracking() on cycle.StokvelId equals stokvel.Id
            where stokvelIds.Contains(cycle.StokvelId) && cycle.PayoutProcessedAtUtc.HasValue
            select new { Cycle = cycle, StokvelName = stokvel.Name };

        if (cursor is not null)
        {
            var time = cursor.LastOccurredAtUtc;
            if (2 < cursor.LastTypeOrder)
                payouts = payouts.Where(x => x.Cycle.PayoutProcessedAtUtc!.Value <= time);
            else if (2 > cursor.LastTypeOrder)
                payouts = payouts.Where(x => x.Cycle.PayoutProcessedAtUtc!.Value < time);
            else
            {
                var stokvelId = cursor.LastStokvelId;
                var entityId = cursor.LastEntityId;
                payouts = payouts.Where(x =>
                    x.Cycle.PayoutProcessedAtUtc!.Value < time ||
                    (x.Cycle.PayoutProcessedAtUtc!.Value == time &&
                     (x.Cycle.StokvelId.CompareTo(stokvelId) < 0 ||
                      (x.Cycle.StokvelId == stokvelId &&
                       x.Cycle.Id.CompareTo(entityId) < 0))));
            }
        }

        var payoutRows = await payouts
            .OrderByDescending(x => x.Cycle.PayoutProcessedAtUtc)
            .ThenByDescending(x => x.Cycle.StokvelId)
            .ThenByDescending(x => x.Cycle.Id)
            .Take(size + 1)
            .Select(x => new ActivityRow(
                x.Cycle.Id, x.Cycle.StokvelId, x.StokvelName,
                "Payout", 2, x.Cycle.PayoutProcessedAtUtc!.Value,
                null, null, null, x.Cycle.PeriodNumber))
            .ToListAsync(cancellationToken);

        var joins =
            from membership in db.StokvelMembers.AsNoTracking()
            join stokvel in db.Stokvels.AsNoTracking() on membership.StokvelId equals stokvel.Id
            join member in db.Users.AsNoTracking() on membership.UserId equals member.Id
            where stokvelIds.Contains(membership.StokvelId)
            select new { Membership = membership, StokvelName = stokvel.Name, MemberName = member.FullName };

        if (cursor is not null)
        {
            var time = cursor.LastOccurredAtUtc;
            if (1 < cursor.LastTypeOrder)
                joins = joins.Where(x => x.Membership.JoinedAtUtc <= time);
            else if (1 > cursor.LastTypeOrder)
                joins = joins.Where(x => x.Membership.JoinedAtUtc < time);
            else
            {
                var stokvelId = cursor.LastStokvelId;
                var entityId = cursor.LastEntityId;
                joins = joins.Where(x =>
                    x.Membership.JoinedAtUtc < time ||
                    (x.Membership.JoinedAtUtc == time &&
                     (x.Membership.StokvelId.CompareTo(stokvelId) < 0 ||
                      (x.Membership.StokvelId == stokvelId &&
                       x.Membership.UserId.CompareTo(entityId) < 0))));
            }
        }

        var joinRows = await joins
            .OrderByDescending(x => x.Membership.JoinedAtUtc)
            .ThenByDescending(x => x.Membership.StokvelId)
            .ThenByDescending(x => x.Membership.UserId)
            .Take(size + 1)
            .Select(x => new ActivityRow(
                x.Membership.UserId, x.Membership.StokvelId, x.StokvelName,
                "MemberJoined", 1, x.Membership.JoinedAtUtc,
                x.Membership.UserId, x.MemberName, null, null))
            .ToListAsync(cancellationToken);

        var rows = contributionRows.Concat(payoutRows).Concat(joinRows)
            .OrderByDescending(row => row.OccurredAtUtc)
            .ThenByDescending(row => row.TypeOrder)
            .ThenByDescending(row => row.StokvelId)
            .ThenByDescending(row => row.EntityId)
            .Take(size + 1)
            .ToList();

        var hasMore = rows.Count > size;
        var page = rows.Take(size).ToList();
        var items = page.Select(ToResponse).ToList();
        var next = string.Empty;
        if (hasMore && page.Count > 0)
        {
            var last = page[^1];
            next = new ActivityFeedPageToken(
                1, userId, last.OccurredAtUtc, last.TypeOrder,
                last.StokvelId, last.EntityId).Encode();
        }

        return Ok(new ActivityFeedPageResponse(items, next, size));
    }

    private static ActivityFeedItemResponse ToResponse(ActivityRow row)
    {
        var key = $"{row.TypeOrder}:{row.StokvelId:N}:{row.EntityId:N}";
        var summary = row.Type switch
        {
            "Contribution" => $"{row.RelatedUserName} made a contribution",
            "Payout" => $"Payout processed for cycle {row.CycleNumber}",
            _ => $"{row.RelatedUserName} joined the stokvel"
        };
        long? amountInCents = row.Amount.HasValue
            ? decimal.ToInt64(decimal.Round(row.Amount.Value * 100m, 0, MidpointRounding.AwayFromZero))
            : null;
        return new ActivityFeedItemResponse(
            key, row.StokvelId, row.StokvelName, row.Type,
            DateTime.SpecifyKind(row.OccurredAtUtc, DateTimeKind.Utc), summary,
            row.RelatedUserId, row.RelatedUserName, amountInCents, row.CycleNumber);
    }
}
