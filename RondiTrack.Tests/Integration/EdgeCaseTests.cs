
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RondiTrack.Tests.Integration;
using Xunit;

namespace RondiTrack.Tests;

/// Integration tests that send HTTP requests through the RondiTrack API.
/// ApiTestFactory starts the real application for these tests.
[Collection("RondiTrack PostgreSQL collection")]
public class EdgeCaseTests
{
    private readonly HttpClient _client;

    /// Receives the existing test factory and creates an HTTP client
    /// for sending requests to the application.
    public EdgeCaseTests(ApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// Checks that the stokvel and user list endpoints respond
    /// successfully and return non-empty JSON arrays.
    ///
    /// Note: This is a setup/smoke test. It does not submit an
    /// invalid contribution amount.
    [Fact]
    public async Task ShouldLoadStokvelsAndUsersSuccessfully()
    {
        // Request the list of stokvels from the API.
        var stokvelsResponse =
            await _client.GetAsync("/api/stokvels");

        // The list endpoint should return HTTP 200 OK.
        Assert.Equal(
            HttpStatusCode.OK,
            stokvelsResponse.StatusCode);

        // Deserialize the response body into JSON elements.
        var stokvels =
            await stokvelsResponse.Content
                .ReadFromJsonAsync<List<JsonElement>>();

        // Confirm that the response contains at least one stokvel.
        Assert.NotNull(stokvels);
        Assert.NotEmpty(stokvels!);

        // Request the list of users from the API.
        var usersResponse =
            await _client.GetAsync("/api/users");

        // The users endpoint should also return HTTP 200 OK.
        Assert.Equal(
            HttpStatusCode.OK,
            usersResponse.StatusCode);

        // Deserialize the response body into JSON elements.
        var users =
            await usersResponse.Content
                .ReadFromJsonAsync<List<JsonElement>>();

        // Confirm that the response contains at least one user.
        Assert.NotNull(users);
        Assert.NotEmpty(users!);
    }

    /// Checks that requesting a stokvel with a randomly generated,
    /// unknown ID returns HTTP 404 Not Found.
    [Fact]
    public async Task ShouldReturnNotFoundForUnknownStokvel()
    {
        // Generate an ID that is unlikely to belong to an existing stokvel.
        var unknownStokvelId = Guid.NewGuid();

        // Request the stokvel using the unknown ID.
        var response =
            await _client.GetAsync(
                $"/api/stokvels/{unknownStokvelId}");

        // The API should report that the stokvel was not found.
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    /// Checks that requesting a user with an unknown ID
    /// returns HTTP 404 Not Found.
    [Fact]
    public async Task ShouldReturnNotFoundForUnknownUser()
    {
        // Generate an ID that is unlikely to belong to an existing user.
        var unknownUserId = Guid.NewGuid();

        // Request the user using the unknown ID.
        var response =
            await _client.GetAsync(
                $"/api/users/{unknownUserId}");

        // The API should report that the user was not found.
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    /// Checks that the stokvel list can be loaded and its first
    /// item is represented as a JSON object.
    ///
    /// Important: This is only a preliminary check for the duplicate
    /// contribution scenario. It does not create or submit a duplicate
    /// contribution, so it does not yet test duplicate rejection.
    [Fact]
    public async Task ShouldLoadStokvelForDuplicateContributionScenario()
    {
        // Retrieve the existing stokvels needed for a potential
        // duplicate contribution test.
        var stokvelsResponse =
            await _client.GetAsync("/api/stokvels");

        // Confirm that the API successfully returns the list.
        Assert.Equal(
            HttpStatusCode.OK,
            stokvelsResponse.StatusCode);

        // Read the response as a collection of JSON values.
        var stokvels =
            await stokvelsResponse.Content
                .ReadFromJsonAsync<List<JsonElement>>();

        // A stokvel must exist before it can be used in the scenario.
        Assert.NotNull(stokvels);
        Assert.NotEmpty(stokvels!);

        // Check that the first list item has the expected JSON object shape.
        Assert.Equal(
            JsonValueKind.Object,
            stokvels![0].ValueKind);
    }

    /// Checks that requesting contributions for an unknown stokvel
    /// and cycle returns HTTP 404 Not Found.
    [Fact]
    public async Task ShouldReturnNotFoundForUnknownContributionCycle()
    {
        // Generate IDs for a stokvel and cycle that should not exist.
        var unknownStokvelId = Guid.NewGuid();
        var unknownCycleId = Guid.NewGuid();

        // Request contributions for the unknown stokvel/cycle combination.
        var response =
            await _client.GetAsync(
                $"/api/stokvels/{unknownStokvelId}/cycles/{unknownCycleId}/contributions");

        // The API should report that the requested resource was not found.
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
}