namespace RondiTrack.DTOs.Stokvels;

/// <summary>
/// Represents the data allowed when updating a stokvel.
/// </summary>
public record UpdateStokvelRequest(
    string Name,
    decimal MonthlyContribution);
