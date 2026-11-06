using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace RondiTrack.DTOs.Activity;

/// <summary>Opaque cursor bound to the authenticated member's activity feed.</summary>
public sealed record ActivityFeedPageToken(
    int Version,
    Guid UserId,
    DateTime LastOccurredAtUtc,
    int LastTypeOrder,
    Guid LastStokvelId,
    Guid LastEntityId)
{
    public string Encode() => WebEncoders.Base64UrlEncode(
        JsonSerializer.SerializeToUtf8Bytes(this));

    public static bool TryDecode(string value, out ActivityFeedPageToken? token)
    {
        token = null;
        try
        {
            token = JsonSerializer.Deserialize<ActivityFeedPageToken>(
                WebEncoders.Base64UrlDecode(value));
            return token is not null && token.Version == 1 &&
                token.UserId != Guid.Empty && token.LastOccurredAtUtc != default &&
                token.LastStokvelId != Guid.Empty && token.LastEntityId != Guid.Empty &&
                token.LastTypeOrder is >= 1 and <= 3;
        }
        catch (FormatException) { return false; }
        catch (JsonException) { return false; }
    }
}
