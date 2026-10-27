using RondiTrack.Auth;
using RondiTrack.Services;
using RondiTrack.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using RondiTrack.Domain.Repositories;
using RondiTrack.Infrastructure.Data;
using RondiTrack.API.Data;
using RondiTrack.DTOs.ContributionCycles;
using RondiTrack.DTOs.Contributions;
using RondiTrack.Mappings;
using RondiTrack.Domain.Models;

namespace RondiTrack.Controllers;

/// Manages contribution cycles for stokvels.
[ApiController]
[Route(ApiRoutes.Stokvels + "/{stokvelId:guid}/cycles")]
public class ContributionCyclesController : ControllerBase
{
    private readonly IContributionCycleRepository
        _cycleRepository;

    private readonly IStokvelRepository
        _stokvelRepository;

    private readonly IContributionRepository
        _contributionRepository;

    private readonly IStokvelService _stokvelService;
    private readonly IAuthorizationService _authorizationService;

    public ContributionCyclesController(
        IContributionCycleRepository cycleRepository,
        IStokvelRepository stokvelRepository,
        IContributionRepository contributionRepository,
        IStokvelService stokvelService,
        IAuthorizationService authorizationService)
    {
        _cycleRepository = cycleRepository;
        _stokvelRepository = stokvelRepository;
        _contributionRepository = contributionRepository;
        _stokvelService = stokvelService;
        _authorizationService = authorizationService;
    }

    /// Returns contribution cycles in deterministic, keyset-paginated order.
    [StokvelAuthorize(Policies.StokvelMember)]
    [HttpGet]
    public async Task<ActionResult<ContributionCyclePageResponse>> GetCycles(
        Guid stokvelId,
        [FromQuery] int? pageSize,
        [FromQuery] string? pageToken,
        [FromQuery] string sortBy = "periodNumber",
        [FromQuery] string sortDirection = "desc")
    {
        const int defaultPageSize = 20;
        const int maximumPageSize = 100;
        var allowedQueryParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "pageSize", "pageToken", "sortBy", "sortDirection"
        };
        var unknownParameter = Request.Query.Keys.FirstOrDefault(
            key => !allowedQueryParameters.Contains(key));
        if (unknownParameter is not null)
        {
            return ProblemResponses.BadRequest(
                $"Unsupported query parameter '{unknownParameter}'.",
                HttpContext.Request.Path);
        }

        if (pageSize < 0)
        {
            return ProblemResponses.BadRequest(
                "pageSize cannot be negative.",
                HttpContext.Request.Path);
        }

        var requestedPageSize = pageSize.GetValueOrDefault(defaultPageSize);
        if (requestedPageSize == 0)
            requestedPageSize = defaultPageSize;
        var effectivePageSize = Math.Min(requestedPageSize, maximumPageSize);
        sortBy = sortBy.Trim();
        if (!string.Equals(sortBy, "periodNumber", StringComparison.OrdinalIgnoreCase))
        {
            return ProblemResponses.BadRequest(
                "sortBy must be 'periodNumber'.",
                HttpContext.Request.Path);
        }

        sortDirection = sortDirection.Trim().ToLowerInvariant();
        if (sortDirection is not ("asc" or "desc"))
        {
            return ProblemResponses.BadRequest(
                "sortDirection must be either 'asc' or 'desc'.",
                HttpContext.Request.Path);
        }

        var descending = sortDirection == "desc";
        int? lastPeriodNumber = null;
        Guid? lastId = null;

        if (!string.IsNullOrWhiteSpace(pageToken))
        {
            if (!ContributionCyclePageToken.TryDecode(pageToken, out var token) || token is null)
            {
                return ProblemResponses.BadRequest(
                    "pageToken is malformed or unsupported.",
                    HttpContext.Request.Path);
            }

            if (token.StokvelId != stokvelId ||
                token.Descending != descending ||
                token.LastId == Guid.Empty ||
                token.LastPeriodNumber <= 0)
            {
                return ProblemResponses.BadRequest(
                    "pageToken cannot be reused with a different stokvel or sort order.",
                    HttpContext.Request.Path);
            }

            lastPeriodNumber = token.LastPeriodNumber;
            lastId = token.LastId;
        }

        var fetched = await _cycleRepository.GetPageByStokvelAsync(
            stokvelId,
            new ContributionCyclePageQuery(
                effectivePageSize,
                descending,
                lastPeriodNumber,
                lastId));

        var hasMore = fetched.Count > effectivePageSize;
        var page = fetched.Take(effectivePageSize).ToList();
        var nextPageToken = string.Empty;

        if (hasMore && page.Count > 0)
        {
            var last = page[^1];
            nextPageToken = new ContributionCyclePageToken(
                Version: 1,
                StokvelId: stokvelId,
                Descending: descending,
                LastPeriodNumber: last.PeriodNumber,
                LastId: last.Id).Encode();
        }

        return Ok(new ContributionCyclePageResponse(
            page.Select(cycle => cycle.ToResponse()).ToList(),
            nextPageToken,
            effectivePageSize));
    }

    /// <summary>
    /// Records a contribution in a specific open cycle. The amount is expressed
    /// as integer cents and the Idempotency-Key identifies the payment intent.
    /// </summary>
    [Authorize]
    [EnableRateLimiting(RateLimitPolicies.Contributions)]
    [HttpPost("{cycleId:guid}/contributions")]
    [ProducesResponseType(typeof(ContributionResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ContributionResponse>> RecordContributionForCycleAsync(
        Guid stokvelId,
        Guid cycleId,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        [FromBody] RecordCycleContributionRequest request)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return ProblemResponses.BadRequest(
                "The Idempotency-Key header is required.",
                HttpContext.Request.Path);
        }

        if (request.UserId == Guid.Empty || request.AmountInCents <= 0)
        {
            return ProblemResponses.UnprocessableEntity(
                "A member and a positive amount in integer cents are required.",
                HttpContext.Request.Path);
        }

        var authorization = await _authorizationService.AuthorizeAsync(
            User,
            new ContributionTargetResource(stokvelId, request.UserId),
            Policies.RecordContribution);
        if (!authorization.Succeeded) return Forbid();

        var cycle = await _cycleRepository.GetByIdAsync(cycleId);
        if (cycle is null || cycle.StokvelId != stokvelId)
        {
            return ProblemResponses.NotFound(
                $"Contribution cycle '{cycleId}' was not found for stokvel '{stokvelId}'.",
                HttpContext.Request.Path);
        }

        // The service checks the idempotency cache before the closed-cycle guard.
        // That preserves replay success if the first request committed before the
        // connection dropped and the cycle was then closed.
        var result = await _stokvelService.RecordContributionAsync(
            stokvelId,
            request.UserId,
            idempotencyKey,
            new RecordContributionRequest(cycle.PeriodNumber, request.AmountInCents / 100m));

        Response.Headers["Idempotency-Replayed"] =
            result is RecordContributionResult.ReplayedFromCache ? "true" : "false";

        return result switch
        {
            RecordContributionResult.Recorded recorded =>
                StatusCode(StatusCodes.Status201Created, recorded.Contribution),
            RecordContributionResult.ReplayedFromCache replayed =>
                StatusCode(StatusCodes.Status201Created, replayed.Contribution),
            RecordContributionResult.KeyConflict =>
                ProblemResponses.Conflict(
                    "The Idempotency-Key has already been used with a different request or is currently being processed.",
                    HttpContext.Request.Path),
            RecordContributionResult.StokvelNotFound =>
                ProblemResponses.NotFound(
                    $"Stokvel '{stokvelId}' was not found.", HttpContext.Request.Path),
            RecordContributionResult.UserNotFound =>
                ProblemResponses.NotFound(
                    $"User '{request.UserId}' was not found.", HttpContext.Request.Path),
            RecordContributionResult.UserNotMember =>
                ProblemResponses.Conflict(
                    $"User '{request.UserId}' is not a member of stokvel '{stokvelId}'.", HttpContext.Request.Path),
            RecordContributionResult.DuplicateContribution duplicate =>
                ProblemResponses.Conflict(
                    $"User '{duplicate.UserId}' has already made a contribution for cycle {duplicate.Cycle}.", HttpContext.Request.Path),
            RecordContributionResult.InvalidAmount =>
                ProblemResponses.UnprocessableEntity(
                    "The amount must be positive and no more than two decimal places.", HttpContext.Request.Path),
            RecordContributionResult.InvalidCycle invalidCycle =>
                ProblemResponses.UnprocessableEntity(
                    $"Contribution cycle '{invalidCycle.Cycle}' is invalid.", HttpContext.Request.Path),
            RecordContributionResult.ClosedCycle closedCycle =>
                ProblemResponses.Conflict(
                    $"Contribution cycle '{closedCycle.Cycle}' is closed or its payout has been processed.", HttpContext.Request.Path),
            _ => ProblemResponses.BadRequest(
                "The contribution could not be recorded.", HttpContext.Request.Path)
        };
    }

    /// <summary>
    /// Returns a bounded, keyset-paginated page of contributions for a cycle.
    /// The page token is bound to the cycle, filters and sort order that created it.
    /// </summary>
    [StokvelAuthorize(Policies.StokvelMember)]
    [HttpGet("{cycleId:guid}/contributions")]
    public async Task<ActionResult<ContributionPageResponse>> GetContributions(
        Guid stokvelId,
        Guid cycleId,
        [FromQuery] int? pageSize,
        [FromQuery] string? pageToken,
        [FromQuery] string sortBy = "recordedAt",
        [FromQuery] string sortDirection = "desc",
        [FromQuery] Guid? userId = null)
    {
        const int defaultPageSize = 20;
        const int maximumPageSize = 100;
        var allowedQueryParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "pageSize", "pageToken", "sortBy", "sortDirection", "userId"
        };
        var unknownParameter = Request.Query.Keys.FirstOrDefault(
            key => !allowedQueryParameters.Contains(key));
        if (unknownParameter is not null)
        {
            return ProblemResponses.BadRequest(
                $"Unsupported query parameter '{unknownParameter}'.",
                HttpContext.Request.Path);
        }

        if (pageSize < 0)
        {
            return ProblemResponses.BadRequest(
                "pageSize cannot be negative.",
                HttpContext.Request.Path);
        }

        var effectivePageSize = pageSize.GetValueOrDefault(defaultPageSize);
        if (effectivePageSize == 0)
            effectivePageSize = defaultPageSize;
        if (effectivePageSize > maximumPageSize)
            effectivePageSize = maximumPageSize;

        sortBy = sortBy.Trim();
        if (!string.Equals(sortBy, "recordedAt", StringComparison.OrdinalIgnoreCase))
        {
            return ProblemResponses.BadRequest(
                "sortBy must be 'recordedAt'. Amount sorting is not exposed; contributions are ordered by recorded time and stable ID.",
                HttpContext.Request.Path);
        }
        sortBy = "recordedAt";
        sortDirection = sortDirection.Trim().ToLowerInvariant();
        if (sortDirection is not ("asc" or "desc"))
        {
            return ProblemResponses.BadRequest(
                "sortDirection must be either 'asc' or 'desc'.",
                HttpContext.Request.Path);
        }

        var cycle = await _cycleRepository.GetByIdAsync(cycleId);
        if (cycle is null || cycle.StokvelId != stokvelId)
        {
            return ProblemResponses.NotFound(
                $"Contribution cycle '{cycleId}' was not found for stokvel '{stokvelId}'.",
                HttpContext.Request.Path);
        }

        var descending = sortDirection == "desc";
        DateTime? lastRecordedAt = null;
        Guid? lastId = null;

        if (!string.IsNullOrWhiteSpace(pageToken))
        {
            if (!ContributionPageToken.TryDecode(pageToken, out var token) || token is null)
            {
                return ProblemResponses.BadRequest(
                    "pageToken is malformed or unsupported.",
                    HttpContext.Request.Path);
            }

            if (token.StokvelId != stokvelId ||
                token.CycleId != cycleId ||
                token.UserId != userId ||
                token.SortBy != sortBy ||
                token.Descending != descending ||
                token.LastId == Guid.Empty ||
                token.LastRecordedAt == default)
            {
                return ProblemResponses.BadRequest(
                    "pageToken cannot be reused with a different cycle, filter or sort order.",
                    HttpContext.Request.Path);
            }

            lastRecordedAt = token.LastRecordedAt;
            lastId = token.LastId;
        }

        var query = new ContributionPageQuery(
            effectivePageSize,
            userId,
            descending,
            lastRecordedAt,
            lastId);

        var fetched = await _contributionRepository
            .GetPageByCycleAsync(stokvelId, cycle.PeriodNumber, query);

        var hasMore = fetched.Count > effectivePageSize;
        var page = fetched.Take(effectivePageSize).ToList();
        var nextPageToken = string.Empty;

        if (hasMore && page.Count > 0)
        {
            var last = page[^1];
            nextPageToken = new ContributionPageToken(
                Version: 1,
                StokvelId: stokvelId,
                CycleId: cycleId,
                UserId: userId,
                SortBy: sortBy,
                Descending: descending,
                LastRecordedAt: last.RecordedAt,
                LastId: last.Id).Encode();
        }

        return Ok(new ContributionPageResponse(
            page.Select(contribution => contribution.ToResponse()).ToList(),
            nextPageToken,
            effectivePageSize));
    }

    [StokvelAuthorize(Policies.StokvelMember)]
    [HttpGet("{cycleId:guid}")]
    public async Task<ActionResult<
        ContributionCycleResponse>>
        GetCycle(
            Guid stokvelId,
            Guid cycleId)
    {
        var cycle =
            await _cycleRepository
                .GetByIdAsync(cycleId);

        if (cycle is null)
        {
            return ProblemResponses.NotFound(
                $"Contribution cycle '{cycleId}' was not found.",
                HttpContext.Request.Path);
        }

        return Ok(cycle.ToResponse());
    }

    [StokvelAuthorize(Policies.StokvelManager)]
    [HttpPost]
    public async Task<ActionResult<
        ContributionCycleResponse>>
        CreateCycle(
            Guid stokvelId,
            CreateContributionCycleRequest request)
    {
        var stokvel =
            await _stokvelRepository
                .GetByIdAsync(stokvelId);

        if (stokvel is null)
        {
            return ProblemResponses.NotFound(
                $"Stokvel '{stokvelId}' was not found.",
                HttpContext.Request.Path);
        }

        var cycle = new ContributionCycle(
            stokvelId,
            request.PeriodNumber,
            request.StartDate,
            request.EndDate,
            request.TargetAmount);

        await _cycleRepository.AddAsync(cycle);

        return CreatedAtAction(
            nameof(GetCycle),
            new
            {
                stokvelId,
                cycleId = cycle.Id
            },
            cycle.ToResponse());
    }

    [StokvelAuthorize(Policies.StokvelManager)]
    [HttpPut("{cycleId:guid}")]
    public async Task<IActionResult>
        UpdateCycle(
            Guid stokvelId,
            Guid cycleId,
            UpdateContributionCycleRequest request)
    {
        var cycle =
            await _cycleRepository
                .GetByIdAsync(cycleId);

        if (cycle is null)
        {
            return ProblemResponses.NotFound(
                $"Contribution cycle '{cycleId}' was not found.",
                HttpContext.Request.Path);
        }

        if (request.Version == 0)
        {
            return ProblemResponses.BadRequest(
                "The Version token is required.",
                HttpContext.Request.Path);
        }

        if (request.Version != cycle.Version)
        {
            return ProblemResponses.Conflict(
                "The contribution cycle changed after it was read. Reload it and retry.",
                HttpContext.Request.Path);
        }
        cycle.Update(
            request.PeriodNumber,
            request.StartDate,
            request.EndDate,
            request.TargetAmount);

        await _cycleRepository.UpdateAsync(cycle);

        return NoContent();
    }

    [StokvelAuthorize(Policies.StokvelManager)]
    [HttpDelete("{cycleId:guid}")]
    public async Task<IActionResult>
        DeleteCycle(
            Guid stokvelId,
            Guid cycleId)
    {
        var deleted =
            await _cycleRepository
                .DeleteAsync(cycleId);

        if (!deleted)
        {
            return ProblemResponses.NotFound(
                $"Contribution cycle '{cycleId}' was not found.",
                HttpContext.Request.Path);
        }

        return NoContent();
    }
    /// <summary>Records that the payout for a contribution cycle has been processed.</summary>
    [StokvelAuthorize(Policies.StokvelManager)]
    [HttpPost("{cycleId:guid}/payout")]
    [EnableRateLimiting(RateLimitPolicies.Payouts)]
    public async Task<IActionResult> ProcessPayout(Guid stokvelId, Guid cycleId)
    {
        var cycle = await _cycleRepository.GetByIdAsync(cycleId);
        if (cycle is null || cycle.StokvelId != stokvelId)
            return ProblemResponses.NotFound(
                $"Contribution cycle '{cycleId}' was not found for stokvel '{stokvelId}'.",
                HttpContext.Request.Path);

        cycle.ProcessPayout(DateTime.UtcNow);
        await _cycleRepository.UpdateAsync(cycle);
        return Ok(new { cycle.Id, cycle.StokvelId, cycle.PayoutProcessedAtUtc });
    }

}
