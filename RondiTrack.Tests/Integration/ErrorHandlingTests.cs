using System.Net;
using System.Net.Http.Json;

namespace RondiTrack.Tests.Integration;

/// <summary>
/// Verifies that the API exposes the documented failure contract.
///
/// These tests are important because callers need predictable HTTP status
/// codes and Problem Details responses when requests fail.
/// </summary>
public class ErrorHandlingTests : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client;

    public ErrorHandlingTests(ApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Verifies that a malformed user request is rejected by validation.
    /// </summary>
    [Fact]
    public async Task CreateUser_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange.
        var request = new
        {
            FullName = "Invalid User",
            Email = "not-an-email"
        };

        // Act.
        var response = await _client.PostAsJsonAsync(
            "/api/users",
            request);

        // Assert.
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);
    }

    /// <summary>
    /// Verifies that requesting an unknown user produces a 404 response.
    /// </summary>
    [Fact]
    public async Task GetUnknownUser_ReturnsNotFound()
    {
        // Act.
        var response = await _client.GetAsync(
            $"/api/users/{Guid.NewGuid()}");

        // Assert.
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);
    }

    /// <summary>
    /// Verifies the stokvel contribution business-rule failure.
    /// </summary>
    [Fact]
    public async Task CreateStokvel_WithInvalidContribution_ReturnsBadRequest()
    {
        // Arrange.
        var request = new
        {
            Name = "Invalid Stokvel",
            MonthlyContribution = 100m
        };

        // Act.
        var response = await _client.PostAsJsonAsync(
            "/api/stokvels",
            request);

        // Assert.
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);
    }
}