using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace RondiTrack.DTOs.ContributionCycles;

/// <summary>
/// Opaque continuation token bound to a stokvel and cycle sort order.
/// </summary>
public sealed record ContributionCyclePageToken(
    int Version,
    Guid StokvelId,
    bool Descending,
    int LastPeriodNumber,
    Guid LastId)
{
    public string Encode() => WebEncoders.Base64UrlEncode(
        JsonSerializer.SerializeToUtf8Bytes(this));

    public static bool TryDecode(
        string token,
        out ContributionCyclePageToken? decoded)
    {
        decoded = null;
        try
        {
            decoded = JsonSerializer.Deserialize<ContributionCyclePageToken>(
                WebEncoders.Base64UrlDecode(token));
            return decoded is not null && decoded.Version == 1;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
