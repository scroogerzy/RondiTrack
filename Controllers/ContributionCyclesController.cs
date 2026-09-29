using Microsoft.AspNetCore.Mvc;
using RondiTrack.Common;
using RondiTrack.Data;
using RondiTrack.DTOs.ContributionCycles;
using RondiTrack.Mappings;
using RondiTrack.Models;

namespace RondiTrack.Controllers;
/// <summary>
/// Creates a new user.
/// </summary>
/// <response code="201">User created.</response>
/// <response code="400">Validation failed.</response>
[ProducesResponseType(typeof(DTOs.Users.UserResponse), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ApiController]
[Route("api/stokvels/{stokvelId:guid}/contribution-cycles")]
public class ContributionCyclesController : ControllerBase
{
    private readonly IContributionCycleRepository _cycleRepository;
    private readonly IStokvelRepository _stokvelRepository;

    public ContributionCyclesController(
        IContributionCycleRepository cycleRepository,
        IStokvelRepository stokvelRepository)
    {
        _cycleRepository = cycleRepository;
        _stokvelRepository = stokvelRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContributionCycleResponse>>>
        GetCyclesAsync(Guid stokvelId)
    {
        var stokvel =
            await _stokvelRepository.GetByIdAsync(stokvelId);

        if (stokvel is null)
        {
            return ProblemResponses.NotFound(
                $"Stokvel '{stokvelId}' was not found.",
                HttpContext.Request.Path);
        }

        var cycles =
            await _cycleRepository.GetByStokvelAsync(stokvelId);

        var response = cycles
            .Select(cycle => cycle.ToResponse())
            .ToList();

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContributionCycleResponse>>
        GetCycleByIdAsync(
            Guid stokvelId,
            Guid id)
    {
        var cycle =
            await _cycleRepository.GetByIdAsync(id);

        if (cycle is null || cycle.StokvelId != stokvelId)
        {
            return ProblemResponses.NotFound(
                $"Contribution cycle '{id}' was not found.",
                HttpContext.Request.Path);
        }

        return Ok(cycle.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<ContributionCycleResponse>>
        CreateCycleAsync(
            Guid stokvelId,
            CreateContributionCycleRequest request)
    {
        var stokvel =
            await _stokvelRepository.GetByIdAsync(stokvelId);

        if (stokvel is null)
        {
            return ProblemResponses.NotFound(
                $"Stokvel '{stokvelId}' was not found.",
                HttpContext.Request.Path);
        }

        var existingCycle =
            await _cycleRepository.GetByStokvelAndPeriodAsync(
                stokvelId,
                request.PeriodNumber);

        if (existingCycle is not null)
        {
            return ProblemResponses.Conflict(
                $"Contribution cycle period '{request.PeriodNumber}' already exists for stokvel '{stokvelId}'.",
                HttpContext.Request.Path);
        }

        try
        {
            var cycle = new ContributionCycle(
                stokvelId,
                request.PeriodNumber,
                request.StartDate,
                request.EndDate,
                request.TargetAmount);

            await _cycleRepository.AddAsync(cycle);

            return CreatedAtAction(
                nameof(GetCycleByIdAsync),
                new
                {
                    stokvelId,
                    id = cycle.Id
                },
                cycle.ToResponse());
        }
        catch (ArgumentException ex)
        {
            return ProblemResponses.BadRequest(
                ex.Message,
                HttpContext.Request.Path);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCycleAsync(
        Guid stokvelId,
        Guid id,
        UpdateContributionCycleRequest request)
    {
        var cycle =
            await _cycleRepository.GetByIdAsync(id);

        if (cycle is null || cycle.StokvelId != stokvelId)
        {
            return ProblemResponses.NotFound(
                $"Contribution cycle '{id}' was not found.",
                HttpContext.Request.Path);
        }

        var existingCycle =
            await _cycleRepository.GetByStokvelAndPeriodAsync(
                stokvelId,
                request.PeriodNumber);

        if (existingCycle is not null &&
            existingCycle.Id != id)
        {
            return ProblemResponses.Conflict(
                $"Contribution cycle period '{request.PeriodNumber}' already exists for stokvel '{stokvelId}'.",
                HttpContext.Request.Path);
        }

        try
        {
            cycle.Update(
                request.PeriodNumber,
                request.StartDate,
                request.EndDate,
                request.TargetAmount);

            await _cycleRepository.UpdateAsync(cycle);

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return ProblemResponses.BadRequest(
                ex.Message,
                HttpContext.Request.Path);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCycleAsync(
        Guid stokvelId,
        Guid id)
    {
        var cycle =
            await _cycleRepository.GetByIdAsync(id);

        if (cycle is null || cycle.StokvelId != stokvelId)
        {
            return ProblemResponses.NotFound(
                $"Contribution cycle '{id}' was not found.",
                HttpContext.Request.Path);
        }

        await _cycleRepository.DeleteAsync(id);

        return NoContent();
    }
}