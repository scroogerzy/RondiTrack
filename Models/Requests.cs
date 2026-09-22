namespace RondiTrack.Models;

// Request model used when creating a new User.
public record CreateUserRequest(
    string FullName,
    string Email);

// Request model used when updating an existing User.
public record UpdateUserRequest(
    string FullName,
    string Email);

// Request model used when creating a new Stokvel.
public record CreateStokvelRequest(
    string Name,
    decimal MonthlyContribution);

// Request model used when updating an existing Stokvel.
public record UpdateStokvelRequest(
    string Name,
    decimal MonthlyContribution);