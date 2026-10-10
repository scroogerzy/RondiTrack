
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RondiTrack.Data;
using RondiTrack.DTOs.Contributions;
using RondiTrack.Mappings;
using RondiTrack.Models;

namespace RondiTrack.Services;

/// <summary>
/// Handles business operations involving stokvel membership
/// and contribution recording.
/// </summary>
public class StokvelService : IStokvelService
{
    private readonly IStokvelRepository _stokvelRepository;
    private readonly IUserRepository _userRepository;
    private readonly IContributionRepository _contributionRepository;
    private readonly IIdempotencyStore _idempotencyStore;
    private readonly IStokvelMemberRepository _memberRepository;

    public StokvelService(
        IStokvelRepository stokvelRepository,
        IUserRepository userRepository,
        IContributionRepository contributionRepository,
        IIdempotencyStore idempotencyStore,
        IStokvelMemberRepository memberRepository)
    {
        _stokvelRepository = stokvelRepository;
        _userRepository = userRepository;
        _contributionRepository = contributionRepository;
        _idempotencyStore = idempotencyStore;
        _memberRepository = memberRepository;
    }

    /// <summary>
    /// Adds a membership record after checking the stokvel, user,
    /// duplicate membership, and the maximum membership count.
    /// </summary>
    public async Task<AddMemberResult> AddMemberAsync(
        Guid stokvelId,
        Guid userId)
    {
        var stokvel =
            await _stokvelRepository.GetByIdAsync(stokvelId);

        if (stokvel is null)
            return new AddMemberResult.StokvelNotFound(stokvelId);

        var user =
            await _userRepository.GetByIdAsync(userId);

        if (user is null)
            return new AddMemberResult.UserNotFound(userId);

        // Check the persisted membership table, not MemberIds.
        var existingMembership =
            await _memberRepository.GetAsync(userId, stokvelId);

        if (existingMembership is not null)
            return new AddMemberResult.AlreadyMember(userId);

        // The current business rule limits each stokvel to 20 members.
        var members =
            await _memberRepository.GetByStokvelIdAsync(stokvelId);

        if (members.Count() >= 20)
            return new AddMemberResult.MembershipLimitReached();

        // Persist the relationship using its explicit join entity.
        var membership = new StokvelMember(
            userId,
            stokvelId,
            "Member",
            DateTime.UtcNow);

        await _memberRepository.AddAsync(membership);

        return new AddMemberResult.Added();
    }

    /// <summary>
    /// Removes a persisted membership.
    /// Returns false when the stokvel or membership does not exist.
    /// </summary>
    public async Task<bool> RemoveMemberAsync(
        Guid stokvelId,
        Guid userId)
    {
        var stokvel =
            await _stokvelRepository.GetByIdAsync(stokvelId);

        if (stokvel is null)
            return false;

        var membership =
            await _memberRepository.GetAsync(userId, stokvelId);

        if (membership is null)
            return false;

        return await _memberRepository.DeleteAsync(
            userId,
            stokvelId);
    }

    /// <summary>
    /// Records a contribution after checking the request, persisted
    /// membership, duplicate contribution, and idempotency key.
    /// </summary>
    public async Task<RecordContributionResult> RecordContributionAsync(
        Guid stokvelId,
        Guid userId,
        string idempotencyKey,
        RecordContributionRequest request)
    {
        // Hash the complete request context to detect changed payloads.
        var requestHash = CreateRequestHash(
            stokvelId,
            userId,
            request);

        var existingRecord =
            await _idempotencyStore.FindAsync(idempotencyKey);

        // Replay the stored result when the same key and payload are reused.
        if (existingRecord is not null)
        {
            if (existingRecord.ResponseBody is null ||
                existingRecord.RequestHash != requestHash)
            {
                return new RecordContributionResult.KeyConflict(
                    idempotencyKey);
            }

            return new RecordContributionResult.ReplayedFromCache(
                existingRecord.ResponseBody);
        }

        var stokvel =
            await _stokvelRepository.GetByIdAsync(stokvelId);

        if (stokvel is null)
        {
            return new RecordContributionResult.StokvelNotFound(
                stokvelId);
        }

        var user =
            await _userRepository.GetByIdAsync(userId);

        if (user is null)
        {
            return new RecordContributionResult.UserNotFound(
                userId);
        }

        // Membership must exist in the same persistence system used
        // by the API's membership endpoints.
        var membership =
            await _memberRepository.GetAsync(userId, stokvelId);

        if (membership is null)
        {
            return new RecordContributionResult.UserNotMember(
                stokvelId,
                userId);
        }

        if (request.Cycle <= 0)
        {
            return new RecordContributionResult.InvalidCycle(
                request.Cycle);
        }

        // Preserve the existing business rule: contributions are R500.
        if (request.Amount != 500m)
        {
            return new RecordContributionResult.InvalidAmount(
                request.Amount);
        }

        var existingContribution =
            await _contributionRepository.GetByMemberAndCycleAsync(
                stokvelId,
                userId,
                request.Cycle);

        if (existingContribution is not null)
        {
            return new RecordContributionResult.DuplicateContribution(
                stokvelId,
                userId,
                request.Cycle);
        }

        // Reserve the key before attempting to store the contribution.
        var reserved =
            await _idempotencyStore.TryReserveAsync(idempotencyKey);

        if (!reserved)
        {
            var reservedRecord =
                await _idempotencyStore.FindAsync(idempotencyKey);

            if (reservedRecord is null ||
                reservedRecord.ResponseBody is null ||
                reservedRecord.RequestHash != requestHash)
            {
                return new RecordContributionResult.KeyConflict(
                    idempotencyKey);
            }

            return new RecordContributionResult.ReplayedFromCache(
                reservedRecord.ResponseBody);
        }

        var contribution = new Contribution(
            stokvelId,
            userId,
            request.Cycle,
            request.Amount);

        await _contributionRepository.AddAsync(contribution);

        var response = contribution.ToResponse();

        await _idempotencyStore.SaveAsync(
            idempotencyKey,
            new IdempotencyRecord(
                requestHash,
                response));

        return new RecordContributionResult.Recorded(response);
    }

    /// <summary>
    /// Creates a deterministic SHA-256 hash from the contribution
    /// request context, including the stokvel, user, cycle, and amount.
    /// </summary>
    private static string CreateRequestHash(
        Guid stokvelId,
        Guid userId,
        RecordContributionRequest request)
    {
        var payload = new
        {
            StokvelId = stokvelId,
            UserId = userId,
            request.Cycle,
            request.Amount
        };

        var json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }
}
