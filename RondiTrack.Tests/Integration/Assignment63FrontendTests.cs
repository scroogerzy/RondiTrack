using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using RondiTrack.Tests.TestSupport;
using Xunit;

namespace RondiTrack.Tests.Integration;

[Collection("Postgres collection")]
public sealed class Assignment63FrontendTests
{
    private readonly PostgresApiFactory _factory;

    public Assignment63FrontendTests(PostgresApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Development_preflight_allows_credentials_and_idempotency_header()
    {
        using var client = _factory.CreateUnauthenticatedClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/login");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "content-type,idempotency-key");

        using var response = await client.SendAsync(request);

        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal("http://localhost:3000", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.Equal("true", response.Headers.GetValues("Access-Control-Allow-Credentials").Single());
        Assert.Contains("idempotency-key", string.Join(",", response.Headers.GetValues("Access-Control-Allow-Headers")), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Preflight_from_unconfigured_origin_has_no_allow_origin_header()
    {
        using var client = _factory.CreateUnauthenticatedClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/login");
        request.Headers.Add("Origin", "https://attacker.invalid");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        using var response = await client.SendAsync(request);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Stokvel_not_found_response_keeps_development_cors_headers()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Origin", "http://localhost:3000");

        using var response = await client.GetAsync($"/api/v1/stokvels/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("http://localhost:3000", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }


    [Fact]
    public async Task Home_endpoint_returns_empty_stokvels_for_a_member_with_no_memberships()
    {
        using var client = _factory.CreateUnauthenticatedClient();
        var email = $"home-empty-{Guid.NewGuid():N}@example.test";
        var registration = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            fullName = "Home Empty Test",
            email,
            password = "a-long-test-password-123"
        });
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);

        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password = "a-long-test-password-123"
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        using var loginJson = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var token = loginJson.RootElement.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await client.GetAsync("/api/v1/me/home");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var home = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Empty(home.RootElement.GetProperty("stokvels").EnumerateArray());
    }

    [Fact]
    public async Task Home_endpoint_returns_a_stokvel_collection_for_the_signed_in_member()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/me/home");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(body.RootElement.GetProperty("stokvels").ValueKind == JsonValueKind.Array);
    }
}
