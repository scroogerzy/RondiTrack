namespace RondiTrack.DTOs.Users;

/// <summary>
/// Represents the data required to create a user.
/// </summary>
public record CreateUserRequest(
    string FullName,
    string Email);