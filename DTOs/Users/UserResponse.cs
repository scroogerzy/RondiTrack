namespace RondiTrack.DTOs.Users;

/// <summary>
/// Represents the public HTTP response for a user.
/// </summary>
public record UserResponse(
    Guid Id,
    string FullName,
    string Email);
