using RondiTrack.DTOs.Contributions;
using RondiTrack.DTOs.ContributionCycles;

namespace RondiTrack.Tests.Unit;

public class ContributionPageTokenTests
{
    [Fact]
    public void ContributionToken_RoundTripsQueryAndCursor()
    {
        var token = new ContributionPageToken(
            Version: 1,
            StokvelId: Guid.NewGuid(),
            CycleId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            SortBy: "recordedAt",
            Descending: true,
            LastRecordedAt: new DateTime(2026, 10, 9, 10, 30, 0, DateTimeKind.Utc),
            LastId: Guid.NewGuid());

        Assert.True(ContributionPageToken.TryDecode(token.Encode(), out var decoded));
        Assert.Equal(token, decoded);
    }

    [Fact]
    public void ContributionToken_RejectsMalformedInput()
    {
        Assert.False(ContributionPageToken.TryDecode("not a token!", out _));
    }

    [Fact]
    public void CycleToken_RoundTripsStokvelAndCursor()
    {
        var token = new ContributionCyclePageToken(
            Version: 1,
            StokvelId: Guid.NewGuid(),
            Descending: false,
            LastPeriodNumber: 4,
            LastId: Guid.NewGuid());

        Assert.True(ContributionCyclePageToken.TryDecode(token.Encode(), out var decoded));
        Assert.Equal(token, decoded);
    }
}
