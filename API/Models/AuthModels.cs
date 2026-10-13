namespace RondiTrack.DTOs.Auth;

public sealed record RegisterRequest(string FullName, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record TokenResponse(string AccessToken, DateTime ExpiresAt);
