using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RondiTrack.Auth;
using RondiTrack.Common;
using RondiTrack.Domain.Models;
using RondiTrack.Domain.Calculations;
using RondiTrack.Infrastructure.Data;
using RondiTrack.Services;
using RondiTrack.Services.Assignment91;

namespace RondiTrack.Controllers;

[ApiController]
[Route("api/v1")]
public sealed class Assignment91Controller(RondiTrackDbContext db, IStokvelService stokvelService, IAuthorizationService authorization) : ControllerBase
{
    private Guid CurrentUserId => Guid.TryParse(User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
    private Task<bool> IsTreasurerAsync(Guid stokvelId, Guid? userId = null) =>
        db.StokvelMembers.AsNoTracking().AnyAsync(member => member.StokvelId == stokvelId && member.UserId == (userId ?? CurrentUserId) && member.Role == StokvelRoles.Treasurer);
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private static long BuildPreviewVersion(uint cycleVersion, IReadOnlyCollection<Guid> memberIds, IReadOnlyDictionary<Guid, decimal> paid)
    {
        var state = new StringBuilder().Append(cycleVersion.ToString(CultureInfo.InvariantCulture));
        foreach (var memberId in memberIds.OrderBy(id => id))
            state.Append('|').Append(memberId.ToString("N")).Append(':').Append(paid.GetValueOrDefault(memberId).ToString(CultureInfo.InvariantCulture));
        return BitConverter.ToInt64(SHA256.HashData(Encoding.UTF8.GetBytes(state.ToString())), 0) & long.MaxValue;
    }

    [Authorize]
    [EnableRateLimiting(RateLimitPolicies.Invites)]
    [HttpPost("stokvels/{stokvelId:guid}/invites")]
    public async Task<IActionResult> CreateInvite(Guid stokvelId)
    {
        var access = await authorization.AuthorizeAsync(User, stokvelId, Policies.StokvelManager);
        if (!access.Succeeded || !await IsTreasurerAsync(stokvelId)) return Forbid();
        if (!await db.Stokvels.AnyAsync(x => x.Id == stokvelId)) return NotFound();
        var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
        var now = DateTime.UtcNow;
        db.StokvelInvites.Add(new StokvelInvite(stokvelId, Hash(code), now.AddDays(7), CurrentUserId, now));
        await db.SaveChangesAsync();
        return Ok(new { code, expiresAtUtc = now.AddDays(7) });
    }

    [AllowAnonymous]
    [HttpGet("invites/{code}")]
    public async Task<IActionResult> PreviewInvite(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length > 100) return NotFound();
        var invite = await db.StokvelInvites.AsNoTracking().SingleOrDefaultAsync(x => x.CodeHash == Hash(code));
        if (invite is null) return NotFound(new { code = "unknown" });
        if (invite.ExpiresAtUtc <= DateTime.UtcNow) return StatusCode(410, new { code = "expired" });
        var stokvel = await db.Stokvels.AsNoTracking().SingleOrDefaultAsync(x => x.Id == invite.StokvelId);
        if (stokvel is null) return NotFound();
        var cycle = await db.ContributionCycles.AsNoTracking().Where(x => x.StokvelId == stokvel.Id && x.ClosedAtUtc == null).OrderBy(x => x.PeriodNumber).FirstOrDefaultAsync();
        var memberCount = await db.StokvelMembers.CountAsync(x => x.StokvelId == stokvel.Id);
        return Ok(new { stokvelId = stokvel.Id, stokvelName = stokvel.Name, rules = stokvel.Rules, contributionAmount = stokvel.MonthlyContribution, schedule = cycle is null ? "To be confirmed" : $"{cycle.StartDate:yyyy-MM-dd} to {cycle.EndDate:yyyy-MM-dd}", memberCount });
    }

    [Authorize]
    [EnableRateLimiting(RateLimitPolicies.Invites)]
    [HttpPost("invites/{code}/join")]
    public async Task<IActionResult> JoinInvite(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length > 100) return NotFound();
        var invite = await db.StokvelInvites.AsNoTracking().SingleOrDefaultAsync(x => x.CodeHash == Hash(code));
        if (invite is null) return NotFound(new { code = "unknown" });
        if (invite.ExpiresAtUtc <= DateTime.UtcNow) return StatusCode(410, new { code = "expired" });
        var userId = CurrentUserId;
        if (userId == Guid.Empty) return Unauthorized();
        if (await db.StokvelMembers.AnyAsync(x => x.StokvelId == invite.StokvelId && x.UserId == userId)) return Conflict(new { code = "already-member" });
        AddMemberResult result;
        try { result = await stokvelService.AddMemberAsync(invite.StokvelId, userId); }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { return Conflict(new { code = "already-member" }); }
        return result switch
        {
            AddMemberResult.Added => Ok(new { stokvelId = invite.StokvelId, joined = true }),
            AddMemberResult.AlreadyMember => Conflict(new { code = "already-member" }),
            AddMemberResult.StokvelNotFound => NotFound(),
            AddMemberResult.UserNotFound => NotFound(),
            AddMemberResult.MembershipLimitReached => Conflict(new { detail = "This stokvel has reached its 20-member limit." }),
            _ => Conflict()
        };
    }

    [Authorize]
    [HttpPut("stokvels/{stokvelId:guid}/rules")]
    public async Task<IActionResult> UpdateRules(Guid stokvelId, [FromBody] UpdateStokvelRulesRequest request)
    {
        var access = await authorization.AuthorizeAsync(User, stokvelId, Policies.StokvelManager);
        if (!access.Succeeded || !await IsTreasurerAsync(stokvelId)) return Forbid();
        var stokvel = await db.Stokvels.SingleOrDefaultAsync(x => x.Id == stokvelId);
        if (stokvel is null) return NotFound();
        try { stokvel.UpdateRules(request.Rules); await db.SaveChangesAsync(); return NoContent(); }
        catch (ArgumentException exception) { return BadRequest(new { detail = exception.Message }); }
    }

    [Authorize]
    [HttpGet("stokvels/{stokvelId:guid}/cycles/{cycleId:guid}/close-preview")]
    public async Task<IActionResult> ClosePreview(Guid stokvelId, Guid cycleId)
    {
        var access = await authorization.AuthorizeAsync(User, stokvelId, Policies.StokvelManager);
        if (!access.Succeeded || !await IsTreasurerAsync(stokvelId)) return Forbid();
        var cycle = await db.ContributionCycles.AsNoTracking().SingleOrDefaultAsync(x => x.Id == cycleId && x.StokvelId == stokvelId);
        if (cycle is null) return NotFound();
        var stokvel = await db.Stokvels.AsNoTracking().SingleAsync(x => x.Id == stokvelId);
        var members = await db.StokvelMembers.AsNoTracking().Where(x => x.StokvelId == stokvelId).Select(x => x.UserId).ToListAsync();
        var paid = await db.Contributions.AsNoTracking().Where(x => x.StokvelId == stokvelId && x.Cycle == cycle.PeriodNumber).GroupBy(x => x.UserId).Select(g => new { UserId = g.Key, Amount = g.Sum(x => x.Amount) }).ToDictionaryAsync(x => x.UserId, x => x.Amount);
        var memberDetails = await db.StokvelMembers.AsNoTracking()
            .Where(member => member.StokvelId == stokvelId)
            .Join(db.Users.AsNoTracking(), member => member.UserId, user => user.Id, (member, user) => new
            {
                userId = user.Id,
                memberName = user.FullName
            })
            .ToListAsync();
        var arrears = memberDetails.Select(member => new
        {
            member.userId,
            member.memberName,
            shortfallCents = ArrearCalculator.CalculateShortfallCents(stokvel.MonthlyContribution, paid.GetValueOrDefault(member.userId))
        }).Where(member => member.shortfallCents > 0).ToList();
        var johannesburg = TimeZoneInfo.FindSystemTimeZoneById("Africa/Johannesburg");
        var localPreviewDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, johannesburg);
        var previewVersion = BuildPreviewVersion(cycle.Version, members, paid);
        return Ok(new { stokvelId, cycleId, periodNumber = cycle.PeriodNumber, version = previewVersion, date = localPreviewDate, arrears, totalArrearsCents = arrears.Sum(x => x.shortfallCents), memberCount = members.Count });
    }

    [Authorize]
    [HttpPost("stokvels/{stokvelId:guid}/cycles/{cycleId:guid}/close")]
    public async Task<IActionResult> CloseCycle(Guid stokvelId, Guid cycleId, [FromBody] CloseCycleRequest request, [FromHeader(Name = "Idempotency-Key")] string? key)
    {
        var access = await authorization.AuthorizeAsync(User, stokvelId, Policies.StokvelManager);
        if (!access.Succeeded || !await IsTreasurerAsync(stokvelId)) return Forbid();
        if (string.IsNullOrWhiteSpace(key) || key.Length > 128) return BadRequest(new { detail = "A valid Idempotency-Key is required." });
        var prior = await db.CycleCloseReceipts.AsNoTracking().SingleOrDefaultAsync(x => x.IdempotencyKey == key);
        if (prior is not null)
            return prior.CycleId == cycleId && prior.StokvelId == stokvelId && prior.PreviewVersion == request.Version
                ? Ok(new { closed = true, cycleId, closedAtUtc = prior.ClosedAtUtc, replayed = true })
                : Conflict(new { detail = "This Idempotency-Key was used for a different close request." });
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var cycle = await db.ContributionCycles.SingleOrDefaultAsync(x => x.Id == cycleId && x.StokvelId == stokvelId);
        if (cycle is null) return NotFound();
        if (cycle.ClosedAtUtc is not null) return Conflict(new { code = "already-closed", detail = "This cycle has already been closed." });
        var stokvel = await db.Stokvels.AsNoTracking().SingleAsync(x => x.Id == stokvelId);
        var members = await db.StokvelMembers.AsNoTracking().Where(x => x.StokvelId == stokvelId).Select(x => x.UserId).ToListAsync();
        var paid = await db.Contributions.AsNoTracking().Where(x => x.StokvelId == stokvelId && x.Cycle == cycle.PeriodNumber).GroupBy(x => x.UserId).Select(g => new { UserId = g.Key, Amount = g.Sum(x => x.Amount) }).ToDictionaryAsync(x => x.UserId, x => x.Amount);
        var currentPreviewVersion = BuildPreviewVersion(cycle.Version, members, paid);
        if (currentPreviewVersion != request.Version) return Conflict(new { code = "stale-preview", detail = "The cycle changed after the preview. Refresh and confirm the updated amounts." });
        var now = DateTime.UtcNow;
        foreach (var userId in members)
        {
            var cents = ArrearCalculator.CalculateShortfallCents(stokvel.MonthlyContribution, paid.GetValueOrDefault(userId));
            if (cents > 0) db.ArrearRecords.Add(new ArrearRecord(stokvelId, userId, cycle.PeriodNumber, cents, now));
        }
        if (!cycle.TryClose(now)) return Conflict(new { code = "already-closed" });
        var nextPeriod = checked(cycle.PeriodNumber + 1);
        var nextExists = await db.ContributionCycles.AnyAsync(x => x.StokvelId == stokvelId && x.PeriodNumber == nextPeriod);
        if (!nextExists)
            db.ContributionCycles.Add(new ContributionCycle(stokvelId, nextPeriod, cycle.StartDate.AddMonths(1), cycle.EndDate.AddMonths(1), cycle.TargetAmount));
        db.CycleCloseReceipts.Add(new CycleCloseReceipt(stokvelId, cycleId, key, request.Version, now));
        try { await db.SaveChangesAsync(); await tx.CommitAsync(); }
        catch (DbUpdateConcurrencyException)
        {
            await tx.RollbackAsync();
            var replay = await db.CycleCloseReceipts.AsNoTracking().SingleOrDefaultAsync(x => x.IdempotencyKey == key);
            if (replay is not null && replay.CycleId == cycleId && replay.StokvelId == stokvelId && replay.PreviewVersion == request.Version)
                return Ok(new { closed = true, cycleId, closedAtUtc = replay.ClosedAtUtc, replayed = true });
            return Conflict(new { code = "stale-preview", detail = "The cycle changed. Refresh the preview." });
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            var replay = await db.CycleCloseReceipts.AsNoTracking().SingleOrDefaultAsync(x => x.IdempotencyKey == key);
            if (replay is not null && replay.CycleId == cycleId && replay.StokvelId == stokvelId && replay.PreviewVersion == request.Version)
                return Ok(new { closed = true, cycleId, closedAtUtc = replay.ClosedAtUtc, replayed = true });
            return Conflict(new { detail = "The close request conflicted with another update. Refresh the preview." });
        }
        return Ok(new { closed = true, cycleId, closedAtUtc = now });
    }

    [Authorize]
    [HttpPost("stokvels/{stokvelId:guid}/contributions/{contributionId:guid}/proof")]
    [RequestSizeLimit(5 * 1024 * 1024 + 64 * 1024)]
    public async Task<IActionResult> UploadProof(Guid stokvelId, Guid contributionId, [FromForm] IFormFile file)
    {
        if (file is null || file.Length is <= 0 or > 5 * 1024 * 1024) return BadRequest(new { field = "file", detail = "Choose a file up to 5 MB." });
        var contribution = await db.Contributions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == contributionId && x.StokvelId == stokvelId);
        if (contribution is null) return NotFound();
        if (await db.ContributionCycles.AnyAsync(x => x.StokvelId == stokvelId && x.PeriodNumber == contribution.Cycle && x.ClosedAtUtc != null)) return Conflict(new { detail = "Proof cannot be changed after the cycle is closed." });
        var caller = CurrentUserId;
        var membership = await db.StokvelMembers.AsNoTracking().SingleOrDefaultAsync(x => x.StokvelId == stokvelId && x.UserId == caller);
        if (membership is null) return NotFound();
        if (contribution.UserId != caller && membership.Role != StokvelRoles.Treasurer) return NotFound();
        await using var input = file.OpenReadStream();
        using var memory = new MemoryStream(); await input.CopyToAsync(memory);
        var bytes = memory.ToArray();
        if (!ProofFileValidator.TryGetContentType(bytes, out var type))
            return BadRequest(new { field = "file", detail = "The file contents must be a genuine PDF, PNG or JPEG." });
        var old = await db.ContributionProofs.SingleOrDefaultAsync(x => x.ContributionId == contributionId);
        if (old is not null) old.Replace(caller, Path.GetFileName(file.FileName), type, bytes, DateTime.UtcNow);
        else db.ContributionProofs.Add(new ContributionProof(stokvelId, contributionId, caller, Path.GetFileName(file.FileName), type, bytes, DateTime.UtcNow));
        await db.SaveChangesAsync();
        return Ok(new { uploaded = true });
    }

    [Authorize]
    [HttpDelete("stokvels/{stokvelId:guid}/contributions/{contributionId:guid}/proof")]
    public async Task<IActionResult> RemoveProof(Guid stokvelId, Guid contributionId)
    {
        var contribution = await db.Contributions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == contributionId && x.StokvelId == stokvelId);
        if (contribution is null) return NotFound();
        if (await db.ContributionCycles.AnyAsync(x => x.StokvelId == stokvelId && x.PeriodNumber == contribution.Cycle && x.ClosedAtUtc != null)) return Conflict(new { detail = "Proof cannot be removed after the cycle is closed." });
        var caller = CurrentUserId;
        var membership = await db.StokvelMembers.AsNoTracking().SingleOrDefaultAsync(x => x.StokvelId == stokvelId && x.UserId == caller);
        if (membership is null || (contribution.UserId != caller && membership.Role != StokvelRoles.Treasurer)) return NotFound();
        var proof = await db.ContributionProofs.SingleOrDefaultAsync(x => x.ContributionId == contributionId && x.StokvelId == stokvelId);
        if (proof is null) return NotFound();
        db.ContributionProofs.Remove(proof);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize]
    [HttpGet("stokvels/{stokvelId:guid}/contributions/{contributionId:guid}/proof")]
    public async Task<IActionResult> DownloadProof(Guid stokvelId, Guid contributionId)
    {
        var proof = await db.ContributionProofs.AsNoTracking().SingleOrDefaultAsync(x => x.StokvelId == stokvelId && x.ContributionId == contributionId);
        if (proof is null) return NotFound();
        var membership = await db.StokvelMembers.AsNoTracking().SingleOrDefaultAsync(x => x.StokvelId == stokvelId && x.UserId == CurrentUserId);
        if (membership is null || (proof.UploadedByUserId != CurrentUserId && membership.Role != StokvelRoles.Treasurer)) return NotFound();
        Response.Headers["Cache-Control"] = "private, no-store";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(proof.Content, proof.ContentType, proof.FileName);
    }
}

public sealed record CloseCycleRequest(long Version);
public sealed record UpdateStokvelRulesRequest(string Rules);
