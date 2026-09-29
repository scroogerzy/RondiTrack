using System.Net;
using System.Net.Http.Json;
using RondiTrack.DTOs.Stokvels;
using RondiTrack.DTOs.Users;
using RondiTrack.DTOs.Contributions;

namespace RondiTrack.Tests.Integration;

/// <summary>
/// Edge-case tests discovered by checking validation boundaries,
/// empty collections and interactions between business rules.
/// </summary>
public class EdgeCaseTests : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client;

    public EdgeCaseTests(ApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Edge case 1:
    /// Requesting a collection when there are no assumptions about
    /// its contents should still return a valid collection response.
    /// </summary>
    [Fact]
    public async Task GetUsers_ReturnsCollectionResponse()
    {
        var response = await _client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var users = await response.Content
            .ReadFromJsonAsync<List<UserResponse>>();

        Assert.NotNull(users);
    }

    /// <summary>
    /// Edge case 2:
    /// Cycle zero is the validator boundary because valid cycles
    /// must be greater than zero.
    /// </summary>
    [Fact]
    public async Task CreateContribution_WithCycleZero_IsRejected()
    {
        // Create the resources required to reach the contribution endpoint.
        var userRequest = new CreateUserRequest(
            "Edge Test User",
            $"edge-{Guid.NewGuid():N}@example.com");

        var userResponse = await _client.PostAsJsonAsync(
            "/api/users",
            userRequest);

        userResponse.EnsureSuccessStatusCode();

        var user = await userResponse.Content
            .ReadFromJsonAsync<UserResponse>();

        Assert.NotNull(user);

        var stokvelRequest = new CreateStokvelRequest(
            $"Edge Stokvel {Guid.NewGuid():N}",
            500m);

        var stokvelResponse = await _client.PostAsJsonAsync(
            "/api/stokvels",
            stokvelRequest);

        stokvelResponse.EnsureSuccessStatusCode();

        var stokvel = await stokvelResponse.Content
            .ReadFromJsonAsync<StokvelResponse>();

        Assert.NotNull(stokvel);

        // Add the user as a member first.
        var memberResponse = await _client.PostAsync(
            $"/api/stokvels/{stokvel!.Id}/members/{user!.Id}",
            null);

        Assert.True(
            memberResponse.IsSuccessStatusCode ||
            memberResponse.StatusCode == HttpStatusCode.Conflict);

        // Cycle 0 violates the RecordContribution validator.
        var contributionRequest = new RecordContributionRequest(
            0,
            500m);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/stokvels/{stokvel.Id}/contributions");

        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

        request.Content = JsonContent.Create(contributionRequest);

        var response = await _client.SendAsync(request);

        // It must not be accepted as a successful contribution.
        Assert.NotEqual(
            HttpStatusCode.Created,
            response.StatusCode);
    }

    /// <summary>
    /// Edge case 3:
    /// Looking up a random GUID must produce the documented not-found
    /// response instead of accidentally returning a successful object.
    /// </summary>
    [Fact]
    public async Task GetUnknownUser_ReturnsNotFound()
    {
        var response = await _client.GetAsync(
            $"/api/users/{Guid.NewGuid()}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
}