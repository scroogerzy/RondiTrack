using System.Net;
using System.Net.Http.Json;
using RondiTrack.DTOs.ContributionCycles;
using RondiTrack.DTOs.Stokvels;

namespace RondiTrack.Tests.Integration;

// Week 5 Day 3 pagination contract test: keep fetching with the opaque cursor
// until nextPageToken is empty, and prove no cycle is duplicated or skipped.
[Collection("RondiTrack PostgreSQL collection")]
public sealed class CyclePaginationTests
{
    private readonly HttpClient _client;

    public CyclePaginationTests(ApiTestFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Walking_cycle_pages_returns_each_cycle_once_then_an_empty_token()
    {
        var unique = Guid.NewGuid().ToString("N");
        var stokvelResponse = await _client.PostAsJsonAsync("/api/stokvels", new
        {
            name = $"Cycle Paging Stokvel {unique}",
            monthlyContribution = 500m
        });
        Assert.Equal(HttpStatusCode.Created, stokvelResponse.StatusCode);
        var stokvel = await stokvelResponse.Content.ReadFromJsonAsync<StokvelResponse>();
        Assert.NotNull(stokvel);

        var start = DateTime.UtcNow.Date;
        for (var period = 1; period <= 2; period++)
        {
            var createResponse = await _client.PostAsJsonAsync(
                $"/api/stokvels/{stokvel.Id}/cycles",
                new
                {
                    periodNumber = period,
                    startDate = start.AddMonths(period - 1),
                    endDate = start.AddMonths(period),
                    targetAmount = 1000m
                });
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        }

        var listUrl = $"/api/stokvels/{stokvel.Id}/cycles?pageSize=1&sortBy=periodNumber&sortDirection=asc";
        var firstResponse = await _client.GetAsync(listUrl);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        var first = await firstResponse.Content.ReadFromJsonAsync<ContributionCyclePageResponse>();
        Assert.NotNull(first);
        Assert.Single(first.Items);
        Assert.Equal(1, first.Items[0].PeriodNumber);
        Assert.False(string.IsNullOrEmpty(first.NextPageToken));

        var secondResponse = await _client.GetAsync(
            listUrl + "&pageToken=" + Uri.EscapeDataString(first.NextPageToken));
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        var second = await secondResponse.Content.ReadFromJsonAsync<ContributionCyclePageResponse>();
        Assert.NotNull(second);
        Assert.Single(second.Items);
        Assert.Equal(2, second.Items[0].PeriodNumber);
        Assert.Equal(string.Empty, second.NextPageToken);

        var defaultPage = await _client.GetFromJsonAsync<ContributionCyclePageResponse>(
            $"/api/stokvels/{stokvel.Id}/cycles");
        Assert.NotNull(defaultPage);
        Assert.Equal(20, defaultPage.PageSize);
    }
}
