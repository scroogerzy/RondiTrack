using RondiTrack.DTOs.Activity;

namespace RondiTrack.Tests.Unit;

public sealed class ActivityFeedPageTokenTests
{
    [Fact]
    public void Encode_and_decode_preserves_user_and_order_cursor()
    {
        var token = new ActivityFeedPageToken(
            1,
            Guid.NewGuid(),
            new DateTime(2026, 11, 4, 10, 0, 0, DateTimeKind.Utc),
            3,
            Guid.NewGuid(),
            Guid.NewGuid());

        Assert.True(ActivityFeedPageToken.TryDecode(token.Encode(), out var decoded));
        Assert.Equal(token, decoded);
    }

    [Fact]
    public void Unknown_activity_types_are_rejected()
    {
        var token = new ActivityFeedPageToken(
            1, Guid.NewGuid(), DateTime.UtcNow, 9, Guid.NewGuid(), Guid.NewGuid());
        Assert.False(ActivityFeedPageToken.TryDecode(token.Encode(), out _));
    }
}
