using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RondiTrack.DTOs.Me;
using RondiTrack.Common;
using RondiTrack.Infrastructure.Data;

namespace RondiTrack.Controllers;

[ApiController]
[Route(ApiRoutes.V1 + "/me")]
public sealed class MeController : ControllerBase
{
    private readonly RondiTrackDbContext _db;
    private readonly TimeProvider _timeProvider;

    public MeController(RondiTrackDbContext db, TimeProvider timeProvider)
    {
        _db = db;
        _timeProvider = timeProvider;
    }

    [HttpGet("home")]
    [ProducesResponseType(typeof(HomeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<HomeResponse>> GetHomeAsync(
        CancellationToken cancellationToken)
    {
        var subject = User.FindFirstValue("sub")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out var userId))
            return Unauthorized();

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var memberships = await _db.StokvelMembers
            .AsNoTracking()
            .Where(member => member.UserId == userId)
            .Include(member => member.Stokvel)
                .ThenInclude(stokvel => stokvel.ContributionCycles)
                    .ThenInclude(cycle => cycle.Contributions)
            .OrderBy(member => member.Stokvel.Name)
            .ToListAsync(cancellationToken);

        var arrearsByStokvel = await _db.ArrearRecords
            .AsNoTracking()
            .Where(arrear => arrear.UserId == userId)
            .GroupBy(arrear => arrear.StokvelId)
            .Select(group => new { StokvelId = group.Key, TotalCents = group.Sum(arrear => arrear.ShortfallCents) })
            .ToDictionaryAsync(item => item.StokvelId, item => item.TotalCents, cancellationToken);

        var items = memberships.Select(member =>
        {
            var cycles = member.Stokvel.ContributionCycles
                .OrderBy(cycle => cycle.PeriodNumber)
                .ToList();
            var current = cycles
                .Where(cycle => AsUtc(cycle.StartDate) <= now && AsUtc(cycle.EndDate) >= now)
                .OrderByDescending(cycle => cycle.PeriodNumber)
                .FirstOrDefault();

            var nextCycle = current ?? cycles
                .Where(cycle => AsUtc(cycle.StartDate) > now)
                .OrderBy(cycle => cycle.PeriodNumber)
                .FirstOrDefault();

            var paid = current is null ? 0m : current.Contributions
                .Where(contribution => contribution.UserId == userId)
                .Sum(contribution => contribution.Amount);
            var contributionDue = member.Stokvel.MonthlyContribution;
            var owed = current is null ? 0m : Math.Max(0m, contributionDue - paid);

            return new StokvelHomeItem(
                member.StokvelId,
                member.Stokvel.Name,
                member.Role,
                current?.PeriodNumber,
                ToCents(paid),
                ToCents(owed),
                nextCycle is null ? null : AsUtc(nextCycle.EndDate),
                arrearsByStokvel.GetValueOrDefault(member.StokvelId));
        }).ToArray();

        return Ok(new HomeResponse(userId, items));
    }

    private static long ToCents(decimal amount) =>
        decimal.ToInt64(decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero));

    private static DateTime AsUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
