using System.Net;
using System.Net.Http.Json;
using RondiTrack.DTOs.ContributionCycles;
using RondiTrack.DTOs.Contributions;
using RondiTrack.DTOs.Stokvels;
using RondiTrack.DTOs.Users;
using RondiTrack.Tests.TestSupport;

namespace RondiTrack.Tests.Integration;

[Collection("Postgres collection")]
public sealed class ContributionHistoryTests
{
    private readonly HttpClient _client;

    public ContributionHistoryTests(PostgresApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task History_filters_members_and_utc_range_and_rejects_reused_filter_token()
    {
        var unique = Guid.NewGuid().ToString("N");
        var created = await _client.PostAsJsonAsync("/api/v1/stokvels", new
        {
            name = $"History {unique}", monthlyContribution = 500m
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var stokvel = await created.Content.ReadFromJsonAsync<StokvelResponse>();
        Assert.NotNull(stokvel);

        var memberResponse = await _client.PostAsJsonAsync("/api/v1/users", new
        {
            fullName = $"History Member {unique}", email = $"history-{unique}@example.test"
        });
        Assert.Equal(HttpStatusCode.Created, memberResponse.StatusCode);
        var member = await memberResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(member);
        Assert.Equal(HttpStatusCode.NoContent,
            (await _client.PostAsync($"/api/v1/stokvels/{stokvel.Id}/members/{member.Id}", null)).StatusCode);

        var now = DateTime.UtcNow;
        var cycleResponse = await _client.PostAsJsonAsync($"/api/v1/stokvels/{stokvel.Id}/cycles", new
        {
            periodNumber = 1, startDate = now.Date, endDate = now.Date.AddMonths(1), targetAmount = 1000m
        });
        Assert.Equal(HttpStatusCode.Created, cycleResponse.StatusCode);
        var cycle = await cycleResponse.Content.ReadFromJsonAsync<ContributionCycleResponse>();
        Assert.NotNull(cycle);

        foreach (var userId in new[] { member.Id })
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post, $"/api/v1/stokvels/{stokvel.Id}/cycles/{cycle.Id}/contributions")
            {
                Content = JsonContent.Create(new { userId, amountInCents = 50_000 })
            };
            request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("N"));
            Assert.Equal(HttpStatusCode.Created, (await _client.SendAsync(request)).StatusCode);
        }

        // The stokvel creator is also a member and may record their own contribution.
        var callerId = await ReadCallerIdAsync();
        using (var request = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/stokvels/{stokvel.Id}/cycles/{cycle.Id}/contributions")
        {
            Content = JsonContent.Create(new { userId = callerId, amountInCents = 50_000 })
        })
        {
            request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("N"));
            Assert.Equal(HttpStatusCode.Created, (await _client.SendAsync(request)).StatusCode);
        }

        var from = DateTimeOffset.UtcNow.AddMinutes(-2);
        var to = DateTimeOffset.UtcNow.AddMinutes(2);
        var baseUrl = $"/api/v1/stokvels/{stokvel.Id}/contributions?pageSize=1" +
            $"&from={Uri.EscapeDataString(from.ToString("O"))}" +
            $"&to={Uri.EscapeDataString(to.ToString("O"))}";
        var firstResponse = await _client.GetAsync(baseUrl);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        var first = await firstResponse.Content.ReadFromJsonAsync<ContributionPageResponse>();
        Assert.NotNull(first);
        Assert.Single(first.Items);
        Assert.NotEmpty(first.NextPageToken);

        var next = baseUrl + "&pageToken=" + Uri.EscapeDataString(first.NextPageToken);
        var secondResponse = await _client.GetAsync(next);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        var second = await secondResponse.Content.ReadFromJsonAsync<ContributionPageResponse>();
        Assert.NotNull(second);
        Assert.Single(second.Items);
        Assert.NotEqual(first.Items[0].Id, second.Items[0].Id);

        var changedRange = baseUrl.Replace(
            Uri.EscapeDataString(from.ToString("O")),
            Uri.EscapeDataString(from.AddSeconds(1).ToString("O"))) +
            "&pageToken=" + Uri.EscapeDataString(first.NextPageToken);
        Assert.Equal(HttpStatusCode.BadRequest, (await _client.GetAsync(changedRange)).StatusCode);

        var memberOnly = baseUrl + "&userId=" + member.Id;
        var filtered = await _client.GetAsync(memberOnly);
        Assert.Equal(HttpStatusCode.OK, filtered.StatusCode);
        var filteredPage = await filtered.Content.ReadFromJsonAsync<ContributionPageResponse>();
        Assert.NotNull(filteredPage);
        Assert.All(filteredPage.Items, item => Assert.Equal(member.Id, item.UserId));

        var notMember = await _client.GetAsync(baseUrl + "&userId=" + Guid.NewGuid());
        Assert.Equal(HttpStatusCode.BadRequest, notMember.StatusCode);
        var reversed = await _client.GetAsync(
            $"/api/v1/stokvels/{stokvel.Id}/contributions?from={Uri.EscapeDataString(to.ToString("O"))}" +
            $"&to={Uri.EscapeDataString(from.ToString("O"))}");
        Assert.Equal(HttpStatusCode.BadRequest, reversed.StatusCode);
    }

    private async Task<Guid> ReadCallerIdAsync()
    {
        var response = await _client.GetAsync("/api/v1/me/home");
        response.EnsureSuccessStatusCode();
        using var json = await System.Text.Json.JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
        return json.RootElement.GetProperty("currentUserId").GetGuid();
    }
}
