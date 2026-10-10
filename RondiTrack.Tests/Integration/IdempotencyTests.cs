
using System.Net;
using System.Net.Http.Json;

namespace RondiTrack.Tests.Integration;

/// Verifies the contribution idempotency contract through HTTP.
///
/// The same request with the same key should be safe to retry.
/// Reusing a key with a different payload should be rejected.
[Collection("RondiTrack PostgreSQL collection")]
public class IdempotencyTests
{
    private readonly HttpClient _client;

    /// Creates an HTTP client for testing the real RondiTrack API.
    public IdempotencyTests(ApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// Verifies that retrying a contribution request with the same
    /// Idempotency-Key does not create a second contribution.
    /// The test creates a user, stokvel, and contribution cycle,
    /// adds the user as a member, and submits the same request twice.
    [Fact]
    public async Task SameContributionRequestAndKey_IsIdempotent()
    {
        // Arrange: create a user.
        var userResponse = await _client.PostAsJsonAsync(
            "/api/users",
            new
            {
                FullName = $"Idempotency User {Guid.NewGuid()}",
                Email = $"idempotency-{Guid.NewGuid()}@example.com"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            userResponse.StatusCode);

        // Read the ID of the newly created user.
        var user = await userResponse.Content
            .ReadFromJsonAsync<UserDto>();

        Assert.NotNull(user);

        // Arrange: create a stokvel.
        var stokvelResponse = await _client.PostAsJsonAsync(
            "/api/stokvels",
            new
            {
                Name = $"Idempotency Stokvel {Guid.NewGuid()}",
                MonthlyContribution = 500m
            });

        Assert.Equal(
            HttpStatusCode.Created,
            stokvelResponse.StatusCode);

        // Read the ID of the newly created stokvel.
        var stokvel = await stokvelResponse.Content
            .ReadFromJsonAsync<StokvelDto>();

        Assert.NotNull(stokvel);

        // Arrange: create contribution cycle 1.
        // A contribution must reference an existing stokvel/cycle combination.
        var cycleStartDate = DateTime.UtcNow.Date;
        var cycleEndDate = cycleStartDate.AddMonths(1);

        var cycleResponse = await _client.PostAsJsonAsync(
            $"/api/stokvels/{stokvel.Id}/cycles",
            new
            {
                // Must match Cycle = 1 in the contribution request below.
                PeriodNumber = 1,

                // The end date is on or after the start date.
                StartDate = cycleStartDate,
                EndDate = cycleEndDate,

                // The validator requires a positive target amount.
                TargetAmount = 500m
            });

        // Read the response body before asserting so the real API error
        // is included in the failure message if cycle creation fails.
        var cycleResponseBody =
            await cycleResponse.Content.ReadAsStringAsync();

        Assert.True(
            cycleResponse.StatusCode == HttpStatusCode.Created,
            $"Cycle creation failed. " +
            $"Expected {(int)HttpStatusCode.Created} Created, " +
            $"but received {(int)cycleResponse.StatusCode} " +
            $"{cycleResponse.StatusCode}. " +
            $"Response body: {cycleResponseBody}");

        // Add the user to the stokvel.
        var memberResponse = await _client.PostAsync(
            $"/api/stokvels/{stokvel.Id}/members/{user.Id}",
            null);

        Assert.Equal(
            HttpStatusCode.NoContent,
            memberResponse.StatusCode);

        // Use one idempotency key for both identical requests.
        var key = Guid.NewGuid().ToString();

        var request = new
        {
            Cycle = 1,
            Amount = 500m
        };

        // Act: submit the first contribution request.
        using var firstRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/stokvels/{stokvel.Id}/members/{user.Id}/contributions");

        firstRequest.Headers.Add("Idempotency-Key", key);
        firstRequest.Content = JsonContent.Create(request);

        var firstResponse = await _client.SendAsync(firstRequest);

        // Act: retry with the same key and identical payload.
        using var secondRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/stokvels/{stokvel.Id}/members/{user.Id}/contributions");

        secondRequest.Headers.Add("Idempotency-Key", key);
        secondRequest.Content = JsonContent.Create(request);

        var secondResponse = await _client.SendAsync(secondRequest);

        // Assert: both requests should return HTTP 201 Created.
        // Include each response body if a request fails.
        var firstBody =
            await firstResponse.Content.ReadAsStringAsync();

        Assert.True(
            firstResponse.StatusCode == HttpStatusCode.Created,
            $"First contribution failed. " +
            $"Status: {(int)firstResponse.StatusCode} " +
            $"{firstResponse.StatusCode}. " +
            $"Response body: {firstBody}");

        var secondBody =
            await secondResponse.Content.ReadAsStringAsync();

        Assert.True(
            secondResponse.StatusCode == HttpStatusCode.Created,
            $"Retry contribution failed. " +
            $"Status: {(int)secondResponse.StatusCode} " +
            $"{secondResponse.StatusCode}. " +
            $"Response body: {secondBody}");

        // The retry should return the same contribution representation.
        Assert.Equal(firstBody, secondBody);
    }

    /// DTO used to read the user creation response.
    /// Only the user ID is needed by this test.
    private sealed record UserDto(Guid Id);

    /// DTO used to read the stokvel creation response.
    /// Only the stokvel ID is needed by this test.
    private sealed record StokvelDto(Guid Id);
}
