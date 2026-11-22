namespace RondiTrack.DTOs.Payouts;

/// <summary>One server-ordered recipient in a cycle's payout rotation.</summary>
public sealed record PayoutOrderEntryResponse(
    Guid MemberId,
    string MemberName,
    int Position,
    long AmountInCents,
    DateTime PayoutDateUtc,
    string Status);

public sealed record PayoutOrderResponse(
    Guid StokvelId,
    Guid CycleId,
    int CycleNumber,
    IReadOnlyList<PayoutOrderEntryResponse> Entries);
