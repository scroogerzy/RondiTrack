using Microsoft.AspNetCore.Mvc;
using RondiTrack.Common;
using RondiTrack.Data;
using RondiTrack.DTOs.Users;
using RondiTrack.Mappings;

namespace RondiTrack.Controllers;
/// <summary>
/// Creates a new user.
/// </summary>
/// <response code="201">User created.</response>
/// <response code="400">Validation failed.</response>
[ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repository;

    public UsersController(IUserRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsersAsync()
    {
        var users = await _repository.GetAllAsync();

        var response = users
            .Select(user => user.ToResponse())
            .ToList();

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetUserByIdAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);

        if (user is null)
        {
            return ProblemResponses.NotFound(
                $"User '{id}' was not found.",
                HttpContext.Request.Path);
        }

        return Ok(user.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> CreateUserAsync(
        CreateUserRequest request)
    {
        try
        {
            var user = new RondiTrack.Models.User(
                request.FullName,
                request.Email);

            await _repository.AddAsync(user);

            return CreatedAtAction(
                nameof(GetUserByIdAsync),
                new { id = user.Id },
                user.ToResponse());
        }
        catch (ArgumentException ex)
        {
            return ProblemResponses.BadRequest(
                ex.Message,
                HttpContext.Request.Path);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUserAsync(
        Guid id,
        UpdateUserRequest request)
    {
        var user = await _repository.GetByIdAsync(id);

        if (user is null)
        {
            return ProblemResponses.NotFound(
                $"User '{id}' was not found.",
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

            if (request.Version != user.Version)
            {
                return ProblemResponses.Conflict(
                    "The user changed after it was read. Reload it and retry.",
                    HttpContext.Request.Path);
            }
            user.UpdateFullName(request.FullName);
            user.UpdateEmail(request.Email);

            await _repository.UpdateAsync(user);

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
    public async Task<IActionResult> DeleteUserAsync(Guid id)
    {
        var deleted = await _repository.DeleteAsync(id);

        if (!deleted)
        {
            return ProblemResponses.NotFound(
                $"User '{id}' was not found.",
                HttpContext.Request.Path);
        }

        return NoContent();
    }
}