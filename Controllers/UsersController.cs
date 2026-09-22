using Microsoft.AspNetCore.Mvc;
using RondiTrack.Data;
using RondiTrack.Models;

namespace RondiTrack.Controllers;

// Provides HTTP endpoints for managing Users.
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repository;

    public UsersController(IUserRepository repository)
    {
        _repository = repository;
    }

    // GET: api/users
    // Returns all users.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsersAsync()
    {
        var users = await _repository.GetAllAsync();

        return Ok(users);
    }

    // GET: api/users/{id}
    // Returns one user by ID.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<User>> GetUserByIdAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);

        if (user is null)
            return NotFound();

        return Ok(user);
    }

    // POST: api/users
    // Creates a new user.
    [HttpPost]
    public async Task<ActionResult<User>> CreateUserAsync(
        CreateUserRequest request)
    {
        try
        {
            var user = new User(
                request.FullName,
                request.Email);

            await _repository.AddAsync(user);

            return CreatedAtAction(
                nameof(GetUserByIdAsync),
                new { id = user.Id },
                user);
        }
        catch (ArgumentException ex)
        {
            // Invalid domain data results in HTTP 400.
            return BadRequest(new { error = ex.Message });
        }
    }

    // PUT: api/users/{id}
    // Updates an existing user.
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUserAsync(
        Guid id,
        UpdateUserRequest request)
    {
        var user = await _repository.GetByIdAsync(id);

        if (user is null)
            return NotFound();

        try
        {
            // Business validation remains inside the User entity.
            user.UpdateFullName(request.FullName);
            user.UpdateEmail(request.Email);

            await _repository.UpdateAsync(user);

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // DELETE: api/users/{id}
    // Deletes an existing user.
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUserAsync(Guid id)
    {
        var deleted = await _repository.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}