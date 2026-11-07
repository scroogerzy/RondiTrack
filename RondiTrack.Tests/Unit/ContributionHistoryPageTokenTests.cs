using RondiTrack.DTOs.Contributions;

namespace RondiTrack.Tests.Unit;

public sealed class ContributionHistoryPageTokenTests
{
    [Fact]
    public void Encode_and_decode_preserves_all_query_binding_fields()
    {
        var token = new ContributionHistoryPageToken(
            1,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTime(2026, 9, 30, 22, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 10, 31, 22, 0, 0, DateTimeKind.Utc),
            "recordedAt",
            true,
            new DateTime(2026, 10, 15, 9, 30, 0, DateTimeKind.Utc),
            Guid.NewGuid());

        Assert.True(ContributionHistoryPageToken.TryDecode(token.Encode(), out var decoded));
        Assert.Equal(token, decoded);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-token")]
    [InlineData("%%%not-base64url%%%")]
    public void Invalid_tokens_are_rejected(string value)
    {
        Assert.False(ContributionHistoryPageToken.TryDecode(value, out _));
    }
}
