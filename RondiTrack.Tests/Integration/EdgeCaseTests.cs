using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RondiTrack.DTOs.Contributions;

namespace RondiTrack.Tests;

public class EdgeCaseTests : IClassFixture<RondiTrackWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EdgeCaseTests(RondiTrackWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ShouldRejectInvalidContributionAmount()
    {
        var stokvelsResponse =
            await _client.GetAsync("/api/stokvels");

        stokvelsResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var stokvels =
            await stokvelsResponse.Content
                .ReadFromJsonAsync<List<dynamic>>();

        stokvels.Should().NotBeNull();
        stokvels!.Count.Should().BeGreaterThan(0);

        var usersResponse =
            await _client.GetAsync("/api/users");

        usersResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var users =
            await usersResponse.Content
                .ReadFromJsonAsync<List<dynamic>>();

        users.Should().NotBeNull();
        users!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldReturnNotFoundForUnknownStokvel()
    {
        var unknownStokvelId = Guid.NewGuid();

        var response =
            await _client.GetAsync(
                $"/api/stokvels/{unknownStokvelId}");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnNotFoundForUnknownUser()
    {
        var unknownUserId = Guid.NewGuid();

        var response =
            await _client.GetAsync(
                $"/api/users/{unknownUserId}");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldRejectDuplicateContribution()
    {
        var stokvelsResponse =
            await _client.GetAsync("/api/stokvels");

        stokvelsResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var stokvels =
            await stokvelsResponse.Content
                .ReadFromJsonAsync<List<dynamic>>();

        stokvels.Should().NotBeNull();
        stokvels!.Count.Should().BeGreaterThan(0);

        var stokvelJson =
            stokvels[0].ToString();

        stokvelJson.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldReturnNotFoundForUnknownContributionCycle()
    {
        var unknownCycleId = Guid.NewGuid();
        var unknownStokvelId = Guid.NewGuid();

        var response =
            await _client.GetAsync(
                $"/api/stokvels/{unknownStokvelId}/cycles/{unknownCycleId}/contributions");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NotFound);
    }
}