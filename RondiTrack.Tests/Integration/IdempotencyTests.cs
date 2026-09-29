using System.Net;
using System.Net.Http.Json;

namespace RondiTrack.Tests.Integration;

/// <summary>
/// Verifies the contribution idempotency contract through HTTP.
///
/// The important requirement is that the same request and same key can be
/// safely retried, while the same key with a different payload is rejected.
/// </summary>
public class IdempotencyTests : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client;

    public IdempotencyTests(ApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Verifies that a contribution request can be retried with the
    /// same Idempotency-Key without creating another contribution.
    ///
    /// This test first creates the required user and stokvel, adds the user
    /// as a member, then sends the contribution request twice.
    /// </summary>
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

        Assert.Equal(HttpStatusCode.Created, userResponse.StatusCode);

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

        Assert.Equal(HttpStatusCode.Created, stokvelResponse.StatusCode);

        var stokvel = await stokvelResponse.Content
            .ReadFromJsonAsync<StokvelDto>();

        Assert.NotNull(stokvel);

        // Add the user to the stokvel.
        var memberResponse = await _client.PostAsync(
            $"/api/stokvels/{stokvel.Id}/members/{user.Id}",
            null);

        Assert.Equal(
            HttpStatusCode.NoContent,
            memberResponse.StatusCode);

        // Use one idempotency key for both attempts.
        var key = Guid.NewGuid().ToString();

        var request = new
        {
            Cycle = 1,
            Amount = 500m
        };

        // Act: first contribution request.
        using var firstRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/stokvels/{stokvel.Id}/members/{user.Id}/contributions");

        firstRequest.Headers.Add(
            "Idempotency-Key",
            key);

        firstRequest.Content =
            JsonContent.Create(request);

        var firstResponse =
            await _client.SendAsync(firstRequest);

        // Act: retry the exact same request.
        using var secondRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/stokvels/{stokvel.Id}/members/{user.Id}/contributions");

        secondRequest.Headers.Add(
            "Idempotency-Key",
            key);

        secondRequest.Content =
            JsonContent.Create(request);

        var secondResponse =
            await _client.SendAsync(secondRequest);

        // Assert: both requests return the same successful status.
        Assert.Equal(
            HttpStatusCode.Created,
            firstResponse.StatusCode);

        Assert.Equal(
            HttpStatusCode.Created,
            secondResponse.StatusCode);

        // The response bodies must also represent the same contribution.
        var firstBody =
            await firstResponse.Content.ReadAsStringAsync();

        var secondBody =
            await secondResponse.Content.ReadAsStringAsync();

        Assert.Equal(firstBody, secondBody);
    }

    /// <summary>
    /// Simple DTO used only to read the user creation response.
    /// </summary>
    private sealed record UserDto(Guid Id);

    /// <summary>
    /// Simple DTO used only to read the stokvel creation response.
    /// </summary>
    private sealed record StokvelDto(Guid Id);
}