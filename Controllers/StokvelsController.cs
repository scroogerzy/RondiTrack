
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

/// <summary>
/// Manages stokvels, memberships, and contribution recording.
/// </summary>
[ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ApiController]
[Route("api/[controller]")]
public class StokvelsController : ControllerBase
{
    private readonly IStokvelRepository _stokvelRepository;
    private readonly IUserRepository _userRepository;
    private readonly IStokvelService _stokvelService;
    private readonly IStokvelMemberRepository _stokvelMemberRepository;

    /// <summary>
    /// Receives the repositories and service required by the endpoints.
    /// </summary>
    public StokvelsController(
        IStokvelRepository stokvelRepository,
        IUserRepository userRepository,
        IStokvelService stokvelService,
        IStokvelMemberRepository stokvelMemberRepository)
    {
        _stokvelRepository = stokvelRepository;
        _userRepository = userRepository;
        _stokvelService = stokvelService;
        _stokvelMemberRepository = stokvelMemberRepository;
    }

    /// <summary>
    /// Retrieves all stokvels.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StokvelResponse>>>
        GetStokvelsAsync()
    {
        var stokvels = await _stokvelRepository.GetAllAsync();

        var response = stokvels
            .Select(stokvel => stokvel.ToResponse())
            .ToList();

        return Ok(response);
    }

    /// <summary>
    /// Retrieves one stokvel by its ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StokvelResponse>>
        GetStokvelByIdAsync(Guid id)
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

    /// <summary>
    /// Creates and persists a stokvel.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<StokvelResponse>>
        CreateStokvelAsync(CreateStokvelRequest request)
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

    /// <summary>
    /// Updates an existing stokvel.
    /// </summary>
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
            if (request.Version == 0)
            {
                return ProblemResponses.BadRequest(
                    "The Version token is required.",
                    HttpContext.Request.Path);
            }

            if (request.Version != stokvel.Version)
            {
                return ProblemResponses.Conflict(
                    "The stokvel changed after it was read. Reload it and retry.",
                    HttpContext.Request.Path);
            }
            stokvel.UpdateName(request.Name);
            stokvel.UpdateContribution(request.MonthlyContribution);

            var updated = await _stokvelRepository.UpdateAsync(stokvel);

            if (!updated)
            {
                return ProblemResponses.NotFound(
                    $"Stokvel '{id}' was not found.",
                    HttpContext.Request.Path);
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return ProblemResponses.BadRequest(
                ex.Message,
                HttpContext.Request.Path);
        }
    }

    /// <summary>
    /// Deletes an existing stokvel.
    /// PostgreSQL foreign-key rules remain in force.
    /// </summary>
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

    /// <summary>
    /// Retrieves the users belonging to a stokvel by reading
    /// the persisted StokvelMembers relationship.
    /// </summary>
    [HttpGet("{stokvelId:guid}/members")]
    public async Task<ActionResult<IEnumerable<UserResponse>>>
        GetMembersAsync(Guid stokvelId)
    {
        // Verify that the stokvel exists.
        var stokvel =
            await _stokvelRepository.GetByIdAsync(stokvelId);

        if (stokvel is null)
        {
            return ProblemResponses.NotFound(
                $"Stokvel '{stokvelId}' was not found.",
                HttpContext.Request.Path);
        }

        // Retrieve memberships from the repository, not MemberIds.
        var memberships =
            await _stokvelMemberRepository
                .GetByStokvelIdAsync(stokvelId);

        var members = new List<UserResponse>();

        // Load each associated user and map it to the API response.
        foreach (var membership in memberships)
        {
            var user =
                await _userRepository.GetByIdAsync(membership.UserId);

            if (user is not null)
            {
                members.Add(user.ToResponse());
            }
        }

        return Ok(members);
    }

    /// <summary>
    /// Adds a user to a stokvel using the service's business rules.
    /// </summary>
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

    /// <summary>
    /// Removes a user from a stokvel.
    /// </summary>
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

    /// <summary>
    /// Records a contribution using the supplied idempotency key.
    /// </summary>
    [HttpPost("{stokvelId:guid}/members/{userId:guid}/contributions")]
    public async Task<ActionResult<ContributionResponse>>
        RecordContributionAsync(
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
