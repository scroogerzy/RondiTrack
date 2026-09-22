using Microsoft.AspNetCore.Mvc;
using RondiTrack.Common;
using RondiTrack.Data;
using RondiTrack.DTOs.Contributions;
using RondiTrack.DTOs.Stokvels;
using RondiTrack.DTOs.Users;
using RondiTrack.Mappings;
using RondiTrack.Models;
using RondiTrack.Services;

namespace RondiTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StokvelsController : ControllerBase
{
    private readonly IStokvelRepository _stokvelRepository;
    private readonly IUserRepository _userRepository;
    private readonly IStokvelService _stokvelService;

    public StokvelsController(
        IStokvelRepository stokvelRepository,
        IUserRepository userRepository,
        IStokvelService stokvelService)
    {
        _stokvelRepository = stokvelRepository;
        _userRepository = userRepository;
        _stokvelService = stokvelService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StokvelResponse>>> GetStokvelsAsync()
    {
        var stokvels = await _stokvelRepository.GetAllAsync();

        var response = stokvels
            .Select(stokvel => stokvel.ToResponse())
            .ToList();

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StokvelResponse>> GetStokvelByIdAsync(
        Guid id)
    {
        var stokvel = await _stokvelRepository.GetByIdAsync(id);

        if (stokvel is null)
        {
            return ProblemResponses.NotFound(
                $"Stokvel '{id}' was not found.",
                HttpContext.Request.Path);
        }

        return Ok(stokvel.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<StokvelResponse>> CreateStokvelAsync(
        CreateStokvelRequest request)
    {
        try
        {
            var stokvel = new Stokvel(
                request.Name,
                request.MonthlyContribution);

            await _stokvelRepository.AddAsync(stokvel);

            return CreatedAtAction(
                nameof(GetStokvelByIdAsync),
                new { id = stokvel.Id },
                stokvel.ToResponse());
        }
        catch (ArgumentException ex)
        {
            return ProblemResponses.BadRequest(
                ex.Message,
                HttpContext.Request.Path);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateStokvelAsync(
        Guid id,
        UpdateStokvelRequest request)
    {
        var stokvel = await _stokvelRepository.GetByIdAsync(id);

        if (stokvel is null)
        {
            return ProblemResponses.NotFound(
                $"Stokvel '{id}' was not found.",
                HttpContext.Request.Path);
        }

        try
        {
            stokvel.UpdateName(request.Name);
            stokvel.UpdateContribution(request.MonthlyContribution);

            await _stokvelRepository.UpdateAsync(stokvel);

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
    public async Task<IActionResult> DeleteStokvelAsync(Guid id)
    {
        var deleted = await _stokvelRepository.DeleteAsync(id);

        if (!deleted)
        {
            return ProblemResponses.NotFound(
                $"Stokvel '{id}' was not found.",
                HttpContext.Request.Path);
        }

        return NoContent();
    }

    [HttpGet("{stokvelId:guid}/members")]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetMembersAsync(
        Guid stokvelId)
    {
        var stokvel = await _stokvelRepository.GetByIdAsync(stokvelId);

        if (stokvel is null)
        {
            return ProblemResponses.NotFound(
                $"Stokvel '{stokvelId}' was not found.",
                HttpContext.Request.Path);
        }

        var members = new List<UserResponse>();

        foreach (var memberId in stokvel.MemberIds)
        {
            var user = await _userRepository.GetByIdAsync(memberId);

            if (user is not null)
            {
                members.Add(user.ToResponse());
            }
        }

        return Ok(members);
    }

    [HttpPost("{stokvelId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> AddMemberAsync(
        Guid stokvelId,
        Guid userId)
    {
        var result = await _stokvelService.AddMemberAsync(
            stokvelId,
            userId);

        return result switch
        {
            AddMemberResult.Added =>
                NoContent(),

            AddMemberResult.StokvelNotFound =>
                ProblemResponses.NotFound(
                    $"Stokvel '{stokvelId}' was not found.",
                    HttpContext.Request.Path),

            AddMemberResult.UserNotFound =>
                ProblemResponses.NotFound(
                    $"User '{userId}' was not found.",
                    HttpContext.Request.Path),

            AddMemberResult.AlreadyMember =>
                ProblemResponses.Conflict(
                    $"User '{userId}' is already a member of stokvel '{stokvelId}'.",
                    HttpContext.Request.Path),

            AddMemberResult.MembershipLimitReached =>
                ProblemResponses.Conflict(
                    "A stokvel may not exceed 20 members.",
                    HttpContext.Request.Path),

            _ =>
                ProblemResponses.BadRequest(
                    "The membership operation could not be completed.",
                    HttpContext.Request.Path)
        };
    }

    [HttpDelete("{stokvelId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMemberAsync(
        Guid stokvelId,
        Guid userId)
    {
        var removed = await _stokvelService.RemoveMemberAsync(
            stokvelId,
            userId);

        if (!removed)
        {
            return ProblemResponses.NotFound(
                $"The membership between user '{userId}' and stokvel '{stokvelId}' was not found.",
                HttpContext.Request.Path);
        }

        return NoContent();
    }

    [HttpPost("{stokvelId:guid}/members/{userId:guid}/contributions")]
    public async Task<ActionResult<ContributionResponse>> RecordContributionAsync(
        Guid stokvelId,
        Guid userId,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        RecordContributionRequest request)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return ProblemResponses.BadRequest(
                "The Idempotency-Key header is required.",
                HttpContext.Request.Path);
        }

        var result = await _stokvelService.RecordContributionAsync(
            stokvelId,
            userId,
            idempotencyKey,
            request);

        return result switch
        {
            RecordContributionResult.Recorded recorded =>
                StatusCode(
                    StatusCodes.Status201Created,
                    recorded.Contribution),

            RecordContributionResult.ReplayedFromCache replayed =>
                StatusCode(
                    StatusCodes.Status201Created,
                    replayed.Contribution),

            RecordContributionResult.KeyConflict =>
                ProblemResponses.Conflict(
                    "The Idempotency-Key has already been used with a different request or is currently being processed.",
                    HttpContext.Request.Path),

            RecordContributionResult.StokvelNotFound =>
                ProblemResponses.NotFound(
                    $"Stokvel '{stokvelId}' was not found.",
                    HttpContext.Request.Path),

            RecordContributionResult.UserNotFound =>
                ProblemResponses.NotFound(
                    $"User '{userId}' was not found.",
                    HttpContext.Request.Path),

            RecordContributionResult.UserNotMember =>
                ProblemResponses.Conflict(
                    $"User '{userId}' is not a member of stokvel '{stokvelId}'.",
                    HttpContext.Request.Path),

            RecordContributionResult.DuplicateContribution duplicate =>
                ProblemResponses.Conflict(
                    $"User '{duplicate.UserId}' has already made a contribution for cycle {duplicate.Cycle}.",
                    HttpContext.Request.Path),

            RecordContributionResult.InvalidAmount invalidAmount =>
                ProblemResponses.UnprocessableEntity(
                    $"Contribution amount '{invalidAmount.Amount}' is invalid. The contribution must be exactly R500.",
                    HttpContext.Request.Path),

            RecordContributionResult.InvalidCycle invalidCycle =>
                ProblemResponses.UnprocessableEntity(
                    $"Contribution cycle '{invalidCycle.Cycle}' is invalid. The cycle must be greater than zero.",
                    HttpContext.Request.Path),

            _ =>
                ProblemResponses.BadRequest(
                    "The contribution could not be recorded.",
                    HttpContext.Request.Path)
        };
    }
}