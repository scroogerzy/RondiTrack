#nullable enable

using System.Linq;
using RondiTrack.Data;
using RondiTrack.DTOs.Contributions;
using RondiTrack.Models;
using RondiTrack.Services;

namespace RondiTrack.Tests.Unit;

/// <summary>
/// Unit tests for StokvelService business rules.
/// These tests call the service directly and do not use HTTP,
/// controllers, WebApplicationFactory, or PostgreSQL.
/// </summary>
public class StokvelServiceTests
{
    /// <summary>
    /// Proves that a user cannot be added to the same stokvel twice.
    /// </summary>
    [Fact]
    public async Task AddMemberAsync_WhenUserAlreadyMember_ReturnsAlreadyMember()
    {
        var stokvelRepository = new InMemoryStokvelRepository();
        var userRepository = new InMemoryUserRepository();
        var contributionRepository = new InMemoryContributionRepository();
        var idempotencyStore = new InMemoryIdempotencyStore();
        var memberRepository = new InMemoryStokvelMemberRepository();

        var stokvel = new Stokvel("Test Stokvel", 500m);
        var user = new User("Test User", "test@example.com");

        await stokvelRepository.AddAsync(stokvel);
        await userRepository.AddAsync(user);

        // Seed the existing relationship in the membership repository.
        await memberRepository.AddAsync(
            new StokvelMember(
                user.Id,
                stokvel.Id,
                "Member",
                DateTime.UtcNow));

        var service = new StokvelService(
            stokvelRepository,
            userRepository,
            contributionRepository,
            idempotencyStore,
            memberRepository);

        var result = await service.AddMemberAsync(
            stokvel.Id,
            user.Id);

        Assert.IsType<AddMemberResult.AlreadyMember>(result);
    }

    /// <summary>
    /// Proves that the same member cannot contribute twice to the same cycle.
    /// </summary>
    [Fact]
    public async Task RecordContributionAsync_WhenContributionAlreadyExists_ReturnsDuplicateContribution()
    {
        var stokvelRepository = new InMemoryStokvelRepository();
        var userRepository = new InMemoryUserRepository();
        var contributionRepository = new InMemoryContributionRepository();
        var idempotencyStore = new InMemoryIdempotencyStore();
        var memberRepository = new InMemoryStokvelMemberRepository();

        var stokvel = new Stokvel("Test Stokvel", 500m);
        var user = new User("Test User", "test@example.com");

        await stokvelRepository.AddAsync(stokvel);
        await userRepository.AddAsync(user);

        // The service now checks the persisted membership abstraction.
        await memberRepository.AddAsync(
            new StokvelMember(
                user.Id,
                stokvel.Id,
                "Member",
                DateTime.UtcNow));

        // Arrange an existing contribution for cycle 1.
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
            idempotencyStore,
            memberRepository);

        var request = new RecordContributionRequest(1, 500m);

        var result = await service.RecordContributionAsync(
            stokvel.Id,
            user.Id,
            "duplicate-test-key",
            request);

        Assert.IsType<RecordContributionResult.DuplicateContribution>(
            result);
    }

    /// <summary>
    /// Proves that an idempotency key cannot be reused with a
    /// different request payload.
    /// </summary>
    [Fact]
    public async Task RecordContributionAsync_WhenSameKeyHasDifferentPayload_ReturnsKeyConflict()
    {
        var stokvelRepository = new InMemoryStokvelRepository();
        var userRepository = new InMemoryUserRepository();
        var contributionRepository = new InMemoryContributionRepository();
        var idempotencyStore = new InMemoryIdempotencyStore();
        var memberRepository = new InMemoryStokvelMemberRepository();

        var stokvel = new Stokvel("Test Stokvel", 500m);
        var user = new User("Test User", "test@example.com");

        await stokvelRepository.AddAsync(stokvel);
        await userRepository.AddAsync(user);

        // Seed the membership required by the contribution service.
        await memberRepository.AddAsync(
            new StokvelMember(
                user.Id,
                stokvel.Id,
                "Member",
                DateTime.UtcNow));

        var service = new StokvelService(
            stokvelRepository,
            userRepository,
            contributionRepository,
            idempotencyStore,
            memberRepository);

        // First request uses cycle 1 and reserves the key.
        var firstRequest = new RecordContributionRequest(1, 500m);

        await service.RecordContributionAsync(
            stokvel.Id,
            user.Id,
            "same-key",
            firstRequest);

        // Reuse the same key with a different cycle.
        var secondRequest = new RecordContributionRequest(2, 500m);

        var result = await service.RecordContributionAsync(
            stokvel.Id,
            user.Id,
            "same-key",
            secondRequest);

        Assert.IsType<RecordContributionResult.KeyConflict>(result);
    }

    /// <summary>
    /// Test double for IStokvelMemberRepository.
    /// It keeps membership records in memory for isolated unit tests.
    /// </summary>
    private sealed class InMemoryStokvelMemberRepository
        : IStokvelMemberRepository
    {
        private readonly List<StokvelMember> _members = new();

        public Task<IEnumerable<StokvelMember>> GetByStokvelIdAsync(
            Guid stokvelId)
        {
            // Return only memberships belonging to the requested stokvel.
            IEnumerable<StokvelMember> result = _members
                .Where(member => member.StokvelId == stokvelId)
                .ToList();

            return Task.FromResult(result);
        }

        public Task<StokvelMember?> GetAsync(
            Guid userId,
            Guid stokvelId)
        {
            var member = _members.FirstOrDefault(
                item => item.UserId == userId &&
                        item.StokvelId == stokvelId);

            return Task.FromResult(member);
        }

        public Task<StokvelMember> AddAsync(StokvelMember member)
        {
            // Prevent duplicate composite keys in the test double.
            var alreadyExists = _members.Any(
                item => item.UserId == member.UserId &&
                        item.StokvelId == member.StokvelId);

            if (alreadyExists)
            {
                throw new InvalidOperationException(
                    "The membership already exists.");
            }

            _members.Add(member);

            return Task.FromResult(member);
        }

        public Task<bool> DeleteAsync(
            Guid userId,
            Guid stokvelId)
        {
            var member = _members.FirstOrDefault(
                item => item.UserId == userId &&
                        item.StokvelId == stokvelId);

            if (member is null)
            {
                return Task.FromResult(false);
            }

            _members.Remove(member);

            return Task.FromResult(true);
        }
    }
}
