using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace RondiTrack.DTOs.Contributions;

/// <summary>
/// Opaque continuation cursor for a stokvel-wide contribution history query.
/// The cursor is bound to every filter that defines the result set.
/// </summary>
public sealed record ContributionHistoryPageToken(
    int Version,
    Guid StokvelId,
    Guid? UserId,
    DateTime? FromUtc,
    DateTime? ToUtc,
    string SortBy,
    bool Descending,
    DateTime LastRecordedAtUtc,
    Guid LastId)
{
    public string Encode() => WebEncoders.Base64UrlEncode(
        JsonSerializer.SerializeToUtf8Bytes(this));

    public static bool TryDecode(string value, out ContributionHistoryPageToken? token)
    {
        token = null;
        try
        {
            token = JsonSerializer.Deserialize<ContributionHistoryPageToken>(
                WebEncoders.Base64UrlDecode(value));
            return token is not null && token.Version == 1 &&
                token.StokvelId != Guid.Empty && token.LastId != Guid.Empty &&
                token.LastRecordedAtUtc != default &&
                string.Equals(token.SortBy, "recordedAt", StringComparison.Ordinal);
        }
        catch (FormatException) { return false; }
        catch (JsonException) { return false; }
    }
}
