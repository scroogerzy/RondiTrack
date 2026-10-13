using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RondiTrack.Tests.TestSupport;
using Xunit;

namespace RondiTrack.Tests.Integration;

[Collection("Postgres collection")]
public sealed class AuthenticationTests
{
    private readonly PostgresApiFactory _factory;
    public AuthenticationTests(PostgresApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Registration_and_login_issue_access_token_and_http_only_refresh_cookie()
    {
        using var client = _factory.CreateUnauthenticatedClient();
        var email = $"auth-{Guid.NewGuid():N}@example.test";
        var registration = await client.PostAsJsonAsync("/api/auth/register", new
        { fullName = "Auth Test User", email, password = "a-long-test-password-123" });
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        { email, password = "a-long-test-password-123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        using var body = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrWhiteSpace(body.RootElement.GetProperty("accessToken").GetString()));
        Assert.Contains(login.Headers.GetValues("Set-Cookie"), value =>
            value.Contains("refresh_token=") && value.Contains("httponly", StringComparison.OrdinalIgnoreCase) &&
            value.Contains("secure", StringComparison.OrdinalIgnoreCase) && value.Contains("Path=/api/auth", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain("refreshToken", body.RootElement.ToString());
    }

    [Fact]
    public async Task Registration_rejects_password_shorter_than_twelve_characters()
    {
        using var client = _factory.CreateUnauthenticatedClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        { fullName = "Short Password", email = $"short-{Guid.NewGuid():N}@example.test", password = "short" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Unknown_email_and_wrong_password_use_the_same_login_problem_detail()
    {
        using var client = _factory.CreateUnauthenticatedClient();
        var email = $"known-{Guid.NewGuid():N}@example.test";
        await client.PostAsJsonAsync("/api/auth/register", new
        { fullName = "Known User", email, password = "a-long-test-password-123" });
        var wrongPassword = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "wrong-password" });
        var unknownEmail = await client.PostAsJsonAsync("/api/auth/login", new { email = $"unknown-{Guid.NewGuid():N}@example.test", password = "wrong-password" });
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownEmail.StatusCode);
        using var wrongJson = JsonDocument.Parse(await wrongPassword.Content.ReadAsStringAsync());
        using var unknownJson = JsonDocument.Parse(await unknownEmail.Content.ReadAsStringAsync());
        foreach (var field in new[] { "type", "title", "status", "detail", "instance" })
            Assert.Equal(wrongJson.RootElement.GetProperty(field).ToString(), unknownJson.RootElement.GetProperty(field).ToString());
    }
    [Fact]
    public async Task Refresh_rotates_tokens_and_reuse_revokes_the_whole_family()
    {
        using var client = _factory.CreateUnauthenticatedClient();
        var email = $"rotation-{Guid.NewGuid():N}@example.test";
        await client.PostAsJsonAsync("/api/auth/register", new
        { fullName = "Rotation User", email, password = "a-long-test-password-123" });
        var login = await client.PostAsJsonAsync("/api/auth/login", new
        { email, password = "a-long-test-password-123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var tokenA = ReadRefreshCookie(login);

        var refreshA = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        refreshA.Headers.Add("Cookie", $"refresh_token={tokenA}");
        var firstRefresh = await client.SendAsync(refreshA);
        Assert.Equal(HttpStatusCode.OK, firstRefresh.StatusCode);
        var tokenB = ReadRefreshCookie(firstRefresh);
        Assert.NotEqual(tokenA, tokenB);

        var replayA = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        replayA.Headers.Add("Cookie", $"refresh_token={tokenA}");
        var replayResponse = await client.SendAsync(replayA);
        Assert.Equal(HttpStatusCode.Unauthorized, replayResponse.StatusCode);
        Assert.Equal("application/problem+json", replayResponse.Content.Headers.ContentType?.MediaType);

        var refreshB = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        refreshB.Headers.Add("Cookie", $"refresh_token={tokenB}");
        var familyRevoked = await client.SendAsync(refreshB);
        Assert.Equal(HttpStatusCode.Unauthorized, familyRevoked.StatusCode);
    }

    [Fact]
    public async Task Protected_endpoint_without_token_returns_401_problem_response()
    {
        using var client = _factory.CreateUnauthenticatedClient();
        var response = await client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Same_cycle_creation_endpoint_returns_404_for_non_member_403_for_member_and_success_for_chairperson()
    {
        using var chairpersonClient = _factory.CreateClient();
        var stokvelResponse = await chairpersonClient.PostAsJsonAsync("/api/stokvels", new
        { name = $"AuthZ Stokvel {Guid.NewGuid():N}", monthlyContribution = 500m });
        Assert.Equal(HttpStatusCode.Created, stokvelResponse.StatusCode);
        using var stokvelJson = JsonDocument.Parse(await stokvelResponse.Content.ReadAsStringAsync());
        var stokvelId = stokvelJson.RootElement.GetProperty("id").GetGuid();

        var memberCreate = await chairpersonClient.PostAsJsonAsync("/api/users", new
        { fullName = "Ordinary Member", email = $"member-{Guid.NewGuid():N}@example.test" });
        using var memberJson = JsonDocument.Parse(await memberCreate.Content.ReadAsStringAsync());
        var memberId = memberJson.RootElement.GetProperty("id").GetGuid();
        var added = await chairpersonClient.PostAsync($"/api/stokvels/{stokvelId}/members/{memberId}", null);
        Assert.Equal(HttpStatusCode.NoContent, added.StatusCode);

        var nonMemberCreate = await chairpersonClient.PostAsJsonAsync("/api/users", new
        { fullName = "Non Member", email = $"nonmember-{Guid.NewGuid():N}@example.test" });
        using var nonMemberJson = JsonDocument.Parse(await nonMemberCreate.Content.ReadAsStringAsync());
        var nonMemberId = nonMemberJson.RootElement.GetProperty("id").GetGuid();

        var cycleRequest = new
        { periodNumber = 1, startDate = DateTime.UtcNow.Date, endDate = DateTime.UtcNow.Date.AddMonths(1), targetAmount = 500m };
        using var nonMemberClient = _factory.CreateClientForUser(nonMemberId);
        var nonMemberResponse = await nonMemberClient.PostAsJsonAsync($"/api/stokvels/{stokvelId}/cycles", cycleRequest);
        Assert.Equal(HttpStatusCode.NotFound, nonMemberResponse.StatusCode);

        using var memberClient = _factory.CreateClientForUser(memberId);
        var memberResponse = await memberClient.PostAsJsonAsync($"/api/stokvels/{stokvelId}/cycles", cycleRequest);
        Assert.Equal(HttpStatusCode.Forbidden, memberResponse.StatusCode);
        Assert.Equal("application/problem+json", memberResponse.Content.Headers.ContentType?.MediaType);

        var chairpersonResponse = await chairpersonClient.PostAsJsonAsync($"/api/stokvels/{stokvelId}/cycles", cycleRequest);
        Assert.Equal(HttpStatusCode.Created, chairpersonResponse.StatusCode);
        using var cycleJson = JsonDocument.Parse(await chairpersonResponse.Content.ReadAsStringAsync());
        var cycleId = cycleJson.RootElement.GetProperty("id").GetGuid();

        var otherUserResponse = await chairpersonClient.PostAsJsonAsync("/api/users", new
        { fullName = "Another Member", email = $"another-{Guid.NewGuid():N}@example.test" });
        using var otherUserJson = JsonDocument.Parse(await otherUserResponse.Content.ReadAsStringAsync());
        var otherUserId = otherUserJson.RootElement.GetProperty("id").GetGuid();
        var addOther = await chairpersonClient.PostAsync($"/api/stokvels/{stokvelId}/members/{otherUserId}", null);
        Assert.Equal(HttpStatusCode.NoContent, addOther.StatusCode);

        using var ownContribution = new HttpRequestMessage(HttpMethod.Post,
            $"/api/stokvels/{stokvelId}/members/{memberId}/contributions")
        { Content = JsonContent.Create(new { cycle = 1, amount = 500m }) };
        ownContribution.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("N"));
        var ownResult = await memberClient.SendAsync(ownContribution);
        Assert.Equal(HttpStatusCode.Created, ownResult.StatusCode);

        using var otherContribution = new HttpRequestMessage(HttpMethod.Post,
            $"/api/stokvels/{stokvelId}/members/{otherUserId}/contributions")
        { Content = JsonContent.Create(new { cycle = 1, amount = 500m }) };
        otherContribution.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("N"));
        var otherResult = await memberClient.SendAsync(otherContribution);
        Assert.Equal(HttpStatusCode.Forbidden, otherResult.StatusCode);

        var memberPayout = await memberClient.PostAsync($"/api/stokvels/{stokvelId}/cycles/{cycleId}/payout", null);
        Assert.Equal(HttpStatusCode.Forbidden, memberPayout.StatusCode);
        var chairpersonPayout = await chairpersonClient.PostAsync($"/api/stokvels/{stokvelId}/cycles/{cycleId}/payout", null);
        Assert.Equal(HttpStatusCode.OK, chairpersonPayout.StatusCode);
        var duplicatePayout = await chairpersonClient.PostAsync($"/api/stokvels/{stokvelId}/cycles/{cycleId}/payout", null);
        Assert.Equal(HttpStatusCode.Conflict, duplicatePayout.StatusCode);
    }

    [Fact]
    public async Task Logout_revokes_refresh_token_and_is_idempotent()
    {
        using var client = _factory.CreateUnauthenticatedClient();
        var email = $"logout-{Guid.NewGuid():N}@example.test";
        await client.PostAsJsonAsync("/api/auth/register", new
        { fullName = "Logout User", email, password = "a-long-test-password-123" });
        var login = await client.PostAsJsonAsync("/api/auth/login", new
        { email, password = "a-long-test-password-123" });
        var token = ReadRefreshCookie(login);

        var logout = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        logout.Headers.Add("Cookie", $"refresh_token={token}");
        var loggedOut = await client.SendAsync(logout);
        Assert.Equal(HttpStatusCode.NoContent, loggedOut.StatusCode);

        var refresh = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        refresh.Headers.Add("Cookie", $"refresh_token={token}");
        var afterLogout = await client.SendAsync(refresh);
        Assert.Equal(HttpStatusCode.Unauthorized, afterLogout.StatusCode);

        var secondLogout = await client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, secondLogout.StatusCode);
    }

    [Fact]
    public async Task Duplicate_email_registration_returns_409_even_when_case_differs()
    {
        using var client = _factory.CreateUnauthenticatedClient();
        var email = $"duplicate-{Guid.NewGuid():N}@example.test";
        var first = await client.PostAsJsonAsync("/api/auth/register", new
        { fullName = "First User", email, password = "a-long-test-password-123" });
        var second = await client.PostAsJsonAsync("/api/auth/register", new
        { fullName = "Second User", email = email.ToUpperInvariant(), password = "a-long-test-password-123" });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    private static string ReadRefreshCookie(HttpResponseMessage response)
    {
        var header = response.Headers.GetValues("Set-Cookie")
            .Single(value => value.StartsWith("refresh_token=", StringComparison.Ordinal));
        var pair = header.Split(';', 2)[0];
        return pair[(pair.IndexOf('=') + 1)..];
    }

}
