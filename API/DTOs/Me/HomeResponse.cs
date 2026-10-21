namespace RondiTrack.DTOs.Me;

/// <summary>
/// Minimal response for the signed-in member's home screen.
/// Monetary values are integer cents to remain exact in JavaScript.
/// </summary>
public sealed record HomeResponse(IReadOnlyList<StokvelHomeItem> Stokvels);

public sealed record StokvelHomeItem(
    Guid StokvelId,
    string Name,
    string Role,
    int? CurrentCycle,
    long AmountPaidThisCycleCents,
    long AmountOwedCents,
    DateTime? NextPayoutDateUtc,
    long ArrearsCents);
