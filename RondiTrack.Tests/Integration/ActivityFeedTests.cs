using System.Net;
using System.Net.Http.Json;
using RondiTrack.DTOs.Activity;
using RondiTrack.DTOs.ContributionCycles;
using RondiTrack.DTOs.Stokvels;
using RondiTrack.DTOs.Users;
using RondiTrack.Tests.TestSupport;

namespace RondiTrack.Tests.Integration;

[Collection("Postgres collection")]
public sealed class ActivityFeedTests
{
    private readonly PostgresApiFactory _factory;

    public ActivityFeedTests(PostgresApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Activity_feed_is_member_scoped_and_keyset_paged()
    {
        using var owner = _factory.CreateClient();
        var unique = Guid.NewGuid().ToString("N");
        var created = await owner.PostAsJsonAsync("/api/v1/stokvels", new
        {
            name = $"Activity {unique}", monthlyContribution = 500m
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var stokvel = await created.Content.ReadFromJsonAsync<StokvelResponse>();
        Assert.NotNull(stokvel);

        var memberResponse = await owner.PostAsJsonAsync("/api/v1/users", new
        {
            fullName = $"Activity Member {unique}", email = $"activity-{unique}@example.test"
        });
        Assert.Equal(HttpStatusCode.Created, memberResponse.StatusCode);
        var member = await memberResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(member);
        Assert.Equal(HttpStatusCode.NoContent,
            (await owner.PostAsync($"/api/v1/stokvels/{stokvel.Id}/members/{member.Id}", null)).StatusCode);

        var now = DateTime.UtcNow;
        var cycleResponse = await owner.PostAsJsonAsync($"/api/v1/stokvels/{stokvel.Id}/cycles", new
        {
            periodNumber = 1, startDate = now.Date, endDate = now.Date.AddMonths(1), targetAmount = 500m
        });
        Assert.Equal(HttpStatusCode.Created, cycleResponse.StatusCode);
        var cycle = await cycleResponse.Content.ReadFromJsonAsync<ContributionCycleResponse>();
        Assert.NotNull(cycle);

        using (var request = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/stokvels/{stokvel.Id}/cycles/{cycle.Id}/contributions")
        {
            Content = JsonContent.Create(new { userId = member.Id, amountInCents = 50_000 })
        })
        {
            request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("N"));
            Assert.Equal(HttpStatusCode.Created, (await owner.SendAsync(request)).StatusCode);
        }

        var firstResponse = await owner.GetAsync("/api/v1/activity?pageSize=1");
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        var first = await firstResponse.Content.ReadFromJsonAsync<ActivityFeedPageResponse>();
        Assert.NotNull(first);
        Assert.Single(first.Items);
        Assert.NotEmpty(first.NextPageToken);

        var secondResponse = await owner.GetAsync(
            "/api/v1/activity?pageSize=1&pageToken=" + Uri.EscapeDataString(first.NextPageToken));
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        var second = await secondResponse.Content.ReadFromJsonAsync<ActivityFeedPageResponse>();
        Assert.NotNull(second);
        Assert.Single(second.Items);
        Assert.NotEqual(first.Items[0].Id, second.Items[0].Id);

        var outsiderUserResponse = await owner.PostAsJsonAsync("/api/v1/users", new
        {
            fullName = $"Activity Outsider {unique}", email = $"activity-outsider-{unique}@example.test"
        });
        Assert.Equal(HttpStatusCode.Created, outsiderUserResponse.StatusCode);
        var outsiderUser = await outsiderUserResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(outsiderUser);

        // A signed-in user with no membership has no access to this group's feed.
        using var outsider = _factory.CreateClientForUser(outsiderUser.Id);
        var outsiderFeed = await outsider.GetAsync("/api/v1/activity?pageSize=50");
        Assert.Equal(HttpStatusCode.OK, outsiderFeed.StatusCode);
        var empty = await outsiderFeed.Content.ReadFromJsonAsync<ActivityFeedPageResponse>();
        Assert.NotNull(empty);
        Assert.Empty(empty.Items);

        var wrongOwnerToken = await outsider.GetAsync(
            "/api/v1/activity?pageSize=1&pageToken=" + Uri.EscapeDataString(first.NextPageToken));
        Assert.Equal(HttpStatusCode.BadRequest, wrongOwnerToken.StatusCode);
    }
}
