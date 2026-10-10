
using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace RondiTrack.Tests.Integration;

/// <summary>
/// Verifies that the API exposes the documented failure contract.
/// </summary>
[Collection("RondiTrack PostgreSQL collection")]
public class ErrorHandlingTests
{
    private readonly HttpClient _client;

    public ErrorHandlingTests(ApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateUser_WithInvalidEmail_ReturnsBadRequest()
    {
        var request = new
        {
            FullName = "Invalid User",
            Email = "not-an-email"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/users",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetUnknownUser_ReturnsNotFound()
    {
        var response = await _client.GetAsync(
            $"/api/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task CreateStokvel_WithInvalidContribution_ReturnsBadRequest()
    {
        var request = new
        {
            Name = "Invalid Stokvel",
            MonthlyContribution = 100m
        };

        var response = await _client.PostAsJsonAsync(
            "/api/stokvels",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);
    }

    /// <summary>
    /// Attempts to create a duplicate period through the API.
    /// The cycle endpoint does not pre-check duplicate period numbers,
    /// so PostgreSQL's unique constraint should produce SQLSTATE 23505.
    /// The central middleware should translate that into HTTP 409.
    /// </summary>
    [Fact]
    public async Task CreateCycle_WithDuplicatePeriodNumber_ReturnsConflict()
    {
        // Arrange: create a new stokvel so this test does not depend
        // on any pre-existing stokvel or cycle in the database.
        var stokvelRequest = new
        {
            Name = $"Unique Constraint Test {Guid.NewGuid():N}",
            MonthlyContribution = 500m
        };

        var stokvelResponse = await _client.PostAsJsonAsync(
            "/api/stokvels",
            stokvelRequest);

        Assert.Equal(HttpStatusCode.Created, stokvelResponse.StatusCode);

        var stokvelJson = await stokvelResponse.Content.ReadAsStringAsync();

        using var stokvelDocument = JsonDocument.Parse(stokvelJson);

        var idProperty = stokvelDocument.RootElement
            .EnumerateObject()
            .FirstOrDefault(property =>
                string.Equals(
                    property.Name,
                    "id",
                    StringComparison.OrdinalIgnoreCase));

        Assert.False(idProperty.Equals(default(JsonProperty)));

        var stokvelId = idProperty.Value.GetGuid();

        var startDate = DateTime.UtcNow.Date;

        var cycleRequest = new
        {
            PeriodNumber = 1,
            StartDate = startDate,
            EndDate = startDate.AddMonths(1),
            TargetAmount = 500m
        };

        // First cycle should be accepted.
        var firstCycleResponse = await _client.PostAsJsonAsync(
            $"/api/stokvels/{stokvelId}/cycles",
            cycleRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            firstCycleResponse.StatusCode);

        // Act: attempt another cycle with the same stokvel and period.
        var duplicateResponse = await _client.PostAsJsonAsync(
            $"/api/stokvels/{stokvelId}/cycles",
            cycleRequest);

        // Assert: PostgreSQL conflict is handled centrally.
        Assert.Equal(
            HttpStatusCode.Conflict,
            duplicateResponse.StatusCode);

        Assert.Equal(
            "application/problem+json",
            duplicateResponse.Content.Headers.ContentType?.MediaType);

        var problemJson =
            await duplicateResponse.Content.ReadAsStringAsync();

        using var problemDocument = JsonDocument.Parse(problemJson);

        Assert.Equal(
            StatusCodes.Status409Conflict,
            problemDocument.RootElement
                .GetProperty("status")
                .GetInt32());

        Assert.Equal(
            "Conflict",
            problemDocument.RootElement
                .GetProperty("title")
                .GetString());
    }
}
