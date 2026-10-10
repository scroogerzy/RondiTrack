using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace RondiTrack.DTOs.Contributions;

/// <summary>
/// Encodes the continuation position and binds it to the query that created it.
/// Clients should treat the token as opaque and return it unchanged.
/// </summary>
public sealed record ContributionPageToken(
    int Version,
    Guid StokvelId,
    Guid CycleId,
    Guid? UserId,
    string SortBy,
    bool Descending,
    DateTime LastRecordedAt,
    Guid LastId)
{
    public string Encode()
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(this);
        return WebEncoders.Base64UrlEncode(json);
    }

    public static bool TryDecode(
        string token,
        out ContributionPageToken? decoded)
    {
        decoded = null;

        try
        {
            var json = WebEncoders.Base64UrlDecode(token);
            decoded = JsonSerializer.Deserialize<ContributionPageToken>(json);
            return decoded is not null &&
                decoded.Version == 1 &&
                decoded.LastId != Guid.Empty &&
                decoded.LastRecordedAt != default;
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
