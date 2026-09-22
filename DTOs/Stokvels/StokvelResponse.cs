namespace RondiTrack.DTOs.Stokvels;

/// <summary>
/// Represents the public HTTP response for a stokvel.
/// </summary>
public record StokvelResponse(
    Guid Id,
    string Name,
    decimal MonthlyContribution,
    int MemberCount);
