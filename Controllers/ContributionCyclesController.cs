using Microsoft.AspNetCore.Mvc;
using RondiTrack.Common;
using RondiTrack.Data;
using RondiTrack.DTOs.ContributionCycles;
using RondiTrack.Mappings;
using RondiTrack.Models;

namespace RondiTrack.Controllers;

/// <summary>
/// Manages contribution cycles for stokvels.
/// </summary>
[ApiController]
[Route("api/stokvels/{stokvelId:guid}/cycles")]
public class ContributionCyclesController : ControllerBase
{
    private readonly IContributionCycleRepository
        _cycleRepository;

    private readonly IStokvelRepository
        _stokvelRepository;

    public ContributionCyclesController(
        IContributionCycleRepository cycleRepository,
        IStokvelRepository stokvelRepository)
    {
        _cycleRepository = cycleRepository;
        _stokvelRepository = stokvelRepository;
    }

    [HttpGet]
    public async Task<ActionResult<
        IEnumerable<ContributionCycleResponse>>>
        GetCycles(Guid stokvelId)
    {
        var cycles =
            await _cycleRepository
                .GetByStokvelAsync(stokvelId);

        return Ok(
            cycles.Select(c => c.ToResponse()));
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