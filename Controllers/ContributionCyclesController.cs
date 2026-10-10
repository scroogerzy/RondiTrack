using Microsoft.AspNetCore.Mvc;
using RondiTrack.Common;
using RondiTrack.Data;
using RondiTrack.DTOs.ContributionCycles;
using RondiTrack.DTOs.Contributions;
using RondiTrack.Mappings;
using RondiTrack.Models;

namespace RondiTrack.Controllers;

/// Manages contribution cycles for stokvels.
[ApiController]
[Route("api/stokvels/{stokvelId:guid}/cycles")]
public class ContributionCyclesController : ControllerBase
{
    private readonly IContributionCycleRepository
        _cycleRepository;

    private readonly IStokvelRepository
        _stokvelRepository;

    private readonly IContributionRepository
        _contributionRepository;

    public ContributionCyclesController(
        IContributionCycleRepository cycleRepository,
        IStokvelRepository stokvelRepository,
        IContributionRepository contributionRepository)
    {
        _cycleRepository = cycleRepository;
        _stokvelRepository = stokvelRepository;
        _contributionRepository = contributionRepository;
    }

    /// Returns contribution cycles in deterministic, keyset-paginated order.
    [HttpGet]
    public async Task<ActionResult<ContributionCyclePageResponse>> GetCycles(
        Guid stokvelId,
        [FromQuery] int? pageSize,
        [FromQuery] string? pageToken,
        [FromQuery] string sortBy = "periodNumber",
        [FromQuery] string sortDirection = "asc")
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
    /// Returns a bounded, keyset-paginated page of contributions for a cycle.
    /// The page token is bound to the cycle, filters and sort order that created it.
    /// </summary>
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
                "sortBy must be 'recordedAt'. Amount sorting is not exposed because the domain model fixes contributions at R500.",
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
}