namespace RondiTrack.DTOs.Users;

/// <summary>
/// Represents the data allowed when updating a user.
/// </summary>
public record UpdateUserRequest(
    string FullName,
    string Email);
