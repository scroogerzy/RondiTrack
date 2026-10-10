using System.Net;
using System.Net.Http.Json;
using RondiTrack.DTOs.ContributionCycles;
using RondiTrack.DTOs.Contributions;
using RondiTrack.DTOs.Stokvels;
using RondiTrack.DTOs.Users;

namespace RondiTrack.Tests.Integration;

// This hits the real endpoint so EF's emitted WHERE / ORDER BY / LIMIT SQL
// appears in the test log for the Assignment 5.3 query-plan evidence.
[Collection("RondiTrack PostgreSQL collection")]
public sealed class ContributionPaginationTests
{
    private readonly HttpClient _client;

    public ContributionPaginationTests(ApiTestFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Walking_pages_returns_each_contribution_once_then_an_empty_token()
    {
        var unique = Guid.NewGuid().ToString("N");
        var stokvelResponse = await _client.PostAsJsonAsync("/api/stokvels", new
        {
            name = $"Paging Stokvel {unique}",
            monthlyContribution = 500m
        });
        Assert.Equal(HttpStatusCode.Created, stokvelResponse.StatusCode);
        var stokvel = await stokvelResponse.Content.ReadFromJsonAsync<StokvelResponse>();
        Assert.NotNull(stokvel);

        var users = new List<UserResponse>();
        for (var memberIndex = 0; memberIndex < 2; memberIndex++)
        {
            var userResponse = await _client.PostAsJsonAsync("/api/users", new
            {
                fullName = $"Paging User {unique} {memberIndex}",
                email = $"paging-{unique}-{memberIndex}@example.test"
            });
            Assert.Equal(HttpStatusCode.Created, userResponse.StatusCode);
            var user = await userResponse.Content.ReadFromJsonAsync<UserResponse>();
            Assert.NotNull(user);
            users.Add(user);

            var memberResponse = await _client.PostAsync(
                $"/api/stokvels/{stokvel.Id}/members/{user.Id}", null);
            Assert.Equal(HttpStatusCode.NoContent, memberResponse.StatusCode);
        }

        var start = DateTime.UtcNow.Date;
        var cycleResponse = await _client.PostAsJsonAsync(
            $"/api/stokvels/{stokvel.Id}/cycles",
            new
            {
                periodNumber = 1,
                startDate = start,
                endDate = start.AddMonths(1),
                targetAmount = 500m
            });
        Assert.Equal(HttpStatusCode.Created, cycleResponse.StatusCode);
        var cycle = await cycleResponse.Content.ReadFromJsonAsync<ContributionCycleResponse>();
        Assert.NotNull(cycle);

        foreach (var user in users)
        {
            using var contributionRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/stokvels/{stokvel.Id}/members/{user.Id}/contributions")
            {
                Content = JsonContent.Create(new { cycle = 1, amount = 500m })
            };
            contributionRequest.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("N"));
            var recorded = await _client.SendAsync(contributionRequest);
            Assert.Equal(HttpStatusCode.Created, recorded.StatusCode);
        }

        var pageUrl = $"/api/stokvels/{stokvel.Id}/cycles/{cycle.Id}/contributions" +
            "?pageSize=1&sortBy=recordedAt&sortDirection=desc";
        var firstResponse = await _client.GetAsync(pageUrl);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        var firstPage = await firstResponse.Content.ReadFromJsonAsync<ContributionPageResponse>();
        Assert.NotNull(firstPage);
        Assert.Single(firstPage.Items);
        Assert.Equal(1, firstPage.PageSize);
        Assert.False(string.IsNullOrEmpty(firstPage.NextPageToken));

        var secondUrl = pageUrl + "&pageToken=" + Uri.EscapeDataString(firstPage.NextPageToken);
        var secondResponse = await _client.GetAsync(secondUrl);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        var secondPage = await secondResponse.Content.ReadFromJsonAsync<ContributionPageResponse>();
        Assert.NotNull(secondPage);
        Assert.Single(secondPage.Items);
        Assert.Equal(string.Empty, secondPage.NextPageToken);
        Assert.NotEqual(firstPage.Items[0].Id, secondPage.Items[0].Id);

        // These inputs are part of the contract, not repository implementation details.
        var negative = await _client.GetAsync(
            $"/api/stokvels/{stokvel.Id}/cycles/{cycle.Id}/contributions?pageSize=-1");
        Assert.Equal(HttpStatusCode.BadRequest, negative.StatusCode);

        var tooLarge = await _client.GetAsync(
            $"/api/stokvels/{stokvel.Id}/cycles/{cycle.Id}/contributions?pageSize=101");
        Assert.Equal(HttpStatusCode.OK, tooLarge.StatusCode);
        var clamped = await tooLarge.Content.ReadFromJsonAsync<ContributionPageResponse>();
        Assert.NotNull(clamped);
        Assert.Equal(100, clamped.PageSize);

        var wrongSort = await _client.GetAsync(
            $"/api/stokvels/{stokvel.Id}/cycles/{cycle.Id}/contributions?pageSize=1&sortBy=amount");
        Assert.Equal(HttpStatusCode.BadRequest, wrongSort.StatusCode);

        var changedSortToken = await _client.GetAsync(
            $"/api/stokvels/{stokvel.Id}/cycles/{cycle.Id}/contributions" +
            "?pageSize=1&sortBy=recordedAt&sortDirection=asc&pageToken=" +
            Uri.EscapeDataString(firstPage.NextPageToken));
        Assert.Equal(HttpStatusCode.BadRequest, changedSortToken.StatusCode);

        // Capture the default page size (20, therefore SQL LIMIT 21) in the real EF log.
        var defaultPageResponse = await _client.GetAsync(
            $"/api/stokvels/{stokvel.Id}/cycles/{cycle.Id}/contributions" +
            "?sortBy=recordedAt&sortDirection=desc");
        Assert.Equal(HttpStatusCode.OK, defaultPageResponse.StatusCode);
        var defaultPage = await defaultPageResponse.Content.ReadFromJsonAsync<ContributionPageResponse>();
        Assert.NotNull(defaultPage);
        Assert.Equal(20, defaultPage.PageSize);
    }
}
