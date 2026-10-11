namespace RondiTrack.DTOs.Stokvels;

/// <summary>
/// Represents the data required to create a stokvel.
/// </summary>
public record CreateStokvelRequest(
    string Name,
    decimal MonthlyContribution);
