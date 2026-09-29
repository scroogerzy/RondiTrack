using RondiTrack.Data;
using RondiTrack.DTOs.Contributions;
using RondiTrack.DTOs.Users;
using RondiTrack.Models;
using RondiTrack.Services;

namespace RondiTrack.Tests.Unit;

/// <summary>
/// Unit tests for StokvelService business rules.
/// These tests call the service directly.
/// They do NOT use HTTP, controllers, or WebApplicationFactory.
/// </summary>
public class StokvelServiceTests
{
    /// <summary>
    /// Proves that a user cannot be added to the same stokvel twice.
    /// Expected business result: AlreadyMember.
    /// </summary>
    [Fact]
    public async Task AddMemberAsync_WhenUserAlreadyMember_ReturnsAlreadyMember()
    {
        var stokvelRepository = new InMemoryStokvelRepository();
        var userRepository = new InMemoryUserRepository();
        var contributionRepository = new InMemoryContributionRepository();
        var idempotencyStore = new InMemoryIdempotencyStore();

        var stokvel = new Stokvel("Test Stokvel", 500m);
        var user = new User("Test User", "test@example.com");

        // Put the user into the stokvel before calling AddMemberAsync.
        stokvel.AddMember(user.Id);

        await stokvelRepository.AddAsync(stokvel);
        await userRepository.AddAsync(user);

        var service = new StokvelService(
            stokvelRepository,
            userRepository,
            contributionRepository,
            idempotencyStore);

        var result = await service.AddMemberAsync(
            stokvel.Id,
            user.Id);

        // Only the business-rule result matters here.
        Assert.IsType<AddMemberResult.AlreadyMember>(result);
    }

    /// <summary>
    /// Proves that the same member cannot contribute twice to the same cycle.
    /// Expected business result: DuplicateContribution.
    /// </summary>
    [Fact]
    public async Task RecordContributionAsync_WhenContributionAlreadyExists_ReturnsDuplicateContribution()
    {
        var stokvelRepository = new InMemoryStokvelRepository();
        var userRepository = new InMemoryUserRepository();
        var contributionRepository = new InMemoryContributionRepository();
        var idempotencyStore = new InMemoryIdempotencyStore();

        var stokvel = new Stokvel("Test Stokvel", 500m);
        var user = new User("Test User", "test@example.com");

        stokvel.AddMember(user.Id);

        await stokvelRepository.AddAsync(stokvel);
        await userRepository.AddAsync(user);

        // Existing contribution for cycle 1.
        var existingContribution = new Contribution(
            stokvel.Id,
            user.Id,
            1,
            500m);

        await contributionRepository.AddAsync(existingContribution);

        var service = new StokvelService(
            stokvelRepository,
            userRepository,
            contributionRepository,
            idempotencyStore);

        // RecordContributionRequest requires BOTH Cycle and Amount.
        var request = new RecordContributionRequest(
            1,
            500m);

        var result = await service.RecordContributionAsync(
            stokvel.Id,
            user.Id,
            "duplicate-test-key",
            request);

        // Only the duplicate-contribution rule is being tested.
        Assert.IsType<RecordContributionResult.DuplicateContribution>(result);
    }

    /// <summary>
    /// Proves that an idempotency key cannot be reused with
    /// a different request payload.
    /// Expected business result: KeyConflict.
    /// </summary>
    [Fact]
    public async Task RecordContributionAsync_WhenSameKeyHasDifferentPayload_ReturnsKeyConflict()
    {
        var stokvelRepository = new InMemoryStokvelRepository();
        var userRepository = new InMemoryUserRepository();
        var contributionRepository = new InMemoryContributionRepository();
        var idempotencyStore = new InMemoryIdempotencyStore();

        var stokvel = new Stokvel("Test Stokvel", 500m);
        var user = new User("Test User", "test@example.com");

        stokvel.AddMember(user.Id);

        await stokvelRepository.AddAsync(stokvel);
        await userRepository.AddAsync(user);

        var service = new StokvelService(
            stokvelRepository,
            userRepository,
            contributionRepository,
            idempotencyStore);

        // First request reserves the idempotency key.
        var firstRequest = new RecordContributionRequest(
            1,
            500m);

        await service.RecordContributionAsync(
            stokvel.Id,
            user.Id,
            "same-key",
            firstRequest);

        // Same key, but DIFFERENT payload.
        var secondRequest = new RecordContributionRequest(
            2,
            500m);

        var result = await service.RecordContributionAsync(
            stokvel.Id,
            user.Id,
            "same-key",
            secondRequest);

        // The key must be rejected because its payload changed.
        Assert.IsType<RecordContributionResult.KeyConflict>(result);
    }
}