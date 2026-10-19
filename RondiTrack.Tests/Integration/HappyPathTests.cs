using RondiTrack.Tests.TestSupport;
using System.Net;
using System.Net.Http.Json;
using RondiTrack.DTOs.Stokvels;
using RondiTrack.DTOs.Users;

namespace RondiTrack.Tests.Integration;

/// <summary>
/// Integration tests for successful API operations.
/// These tests use the real ASP.NET Core pipeline through WebApplicationFactory.
/// </summary>
[Collection("Postgres collection")]
public class HappyPathTests
{
    private readonly HttpClient _client;

    public HappyPathTests(PostgresApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Proves that a valid user can be created through the real API.
    /// This exercises routing, model binding, validation, controller,
    /// repository logic and the HTTP response.
    /// </summary>
    [Fact]
    public async Task CreateUser_WithValidRequest_ReturnsCreated()
    {
        var request = new CreateUserRequest(
            "Integration Test User",
            $"integration-{Guid.NewGuid():N}@example.com");

        var response = await _client.PostAsJsonAsync(
            "/api/v1/users",
            request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    /// <summary>
    /// Proves that a valid stokvel can be created through the real API.
    /// </summary>
    [Fact]
    public async Task CreateStokvel_WithValidRequest_ReturnsCreated()
    {
        var request = new CreateStokvelRequest(
            $"Integration Stokvel {Guid.NewGuid():N}",
            500m);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/stokvels",
            request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    /// <summary>
    /// Proves that the users collection endpoint returns successfully.
    /// </summary>
    [Fact]
    public async Task GetUsers_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/api/v1/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Proves that the stokvel collection endpoint returns successfully.
    /// </summary>
    [Fact]
    public async Task GetStokvels_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/api/v1/stokvels");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
