using Microsoft.AspNetCore.Mvc;
using RondiTrack.Data;
using RondiTrack.Models;

namespace RondiTrack.Controllers;

// Provides HTTP endpoints for managing Stokvels and their memberships.
[ApiController]
[Route("api/[controller]")]
public class StokvelsController : ControllerBase
{
    private readonly IStokvelRepository _stokvelRepository;
    private readonly IUserRepository _userRepository;

    public StokvelsController(
        IStokvelRepository stokvelRepository,
        IUserRepository userRepository)
    {
        _stokvelRepository = stokvelRepository;
        _userRepository = userRepository;
    }

    // GET: api/stokvels
    // Returns all stokvels.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Stokvel>>> GetStokvelsAsync()
    {
        var stokvels = await _stokvelRepository.GetAllAsync();

        return Ok(stokvels);
    }

    // GET: api/stokvels/{id}
    // Returns one stokvel by ID.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Stokvel>> GetStokvelByIdAsync(Guid id)
    {
        var stokvel = await _stokvelRepository.GetByIdAsync(id);

        if (stokvel is null)
            return NotFound();

        return Ok(stokvel);
    }

    // POST: api/stokvels
    // Creates a new stokvel.
    [HttpPost]
    public async Task<ActionResult<Stokvel>> CreateStokvelAsync(
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
                stokvel);
        }
        catch (ArgumentException ex)
        {
            // Invalid domain data results in HTTP 400.
            return BadRequest(new { error = ex.Message });
        }
    }

    // PUT: api/stokvels/{id}
    // Updates an existing stokvel.
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateStokvelAsync(
        Guid id,
        UpdateStokvelRequest request)
    {
        var stokvel = await _stokvelRepository.GetByIdAsync(id);

        if (stokvel is null)
            return NotFound();

        try
        {
            // Business validation remains inside the Stokvel entity.
            stokvel.UpdateName(request.Name);
            stokvel.UpdateContribution(request.MonthlyContribution);

            await _stokvelRepository.UpdateAsync(stokvel);

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // DELETE: api/stokvels/{id}
    // Deletes an existing stokvel.
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteStokvelAsync(Guid id)
    {
        var deleted = await _stokvelRepository.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    // GET: api/stokvels/{stokvelId}/members
    // Returns the users who belong to a stokvel.
    [HttpGet("{stokvelId:guid}/members")]
    public async Task<ActionResult<IEnumerable<User>>> GetMembersAsync(
        Guid stokvelId)
    {
        var stokvel = await _stokvelRepository.GetByIdAsync(stokvelId);

        if (stokvel is null)
            return NotFound();

        var members = new List<User>();

        foreach (var memberId in stokvel.MemberIds)
        {
            var user = await _userRepository.GetByIdAsync(memberId);

            if (user is not null)
                members.Add(user);
        }

        return Ok(members);
    }

    // POST: api/stokvels/{stokvelId}/members/{userId}
    // Adds an existing user to a stokvel.
    [HttpPost("{stokvelId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> AddMemberAsync(
        Guid stokvelId,
        Guid userId)
    {
        var stokvel = await _stokvelRepository.GetByIdAsync(stokvelId);

        if (stokvel is null)
            return NotFound();

        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
            return NotFound();

        try
        {
            // The Stokvel entity enforces duplicate and capacity rules.
            stokvel.AddMember(userId);

            await _stokvelRepository.UpdateAsync(stokvel);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            // Duplicate membership or maximum capacity is a conflict.
            return Conflict(new { error = ex.Message });
        }
    }

    // DELETE: api/stokvels/{stokvelId}/members/{userId}
    // Removes an existing user from a stokvel.
    [HttpDelete("{stokvelId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMemberAsync(
        Guid stokvelId,
        Guid userId)
    {
        var stokvel = await _stokvelRepository.GetByIdAsync(stokvelId);

        if (stokvel is null)
            return NotFound();

        try
        {
            stokvel.RemoveMember(userId);

            await _stokvelRepository.UpdateAsync(stokvel);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }
}