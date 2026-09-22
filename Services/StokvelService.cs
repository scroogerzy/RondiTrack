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

    /// <summary>
    /// Creates a new instance of the stokvel service.
    /// </summary>
    public StokvelService(
        IStokvelRepository stokvelRepository,
        IUserRepository userRepository,
        IContributionRepository contributionRepository,
        IIdempotencyStore idempotencyStore)
    {
        _stokvelRepository = stokvelRepository;
        _userRepository = userRepository;
        _contributionRepository = contributionRepository;
        _idempotencyStore = idempotencyStore;
    }

    /// <inheritdoc />
    public async Task<AddMemberResult> AddMemberAsync(
        Guid stokvelId,
        Guid userId)
    {
        var stokvel = await _stokvelRepository.GetByIdAsync(stokvelId);

        if (stokvel is null)
            return new AddMemberResult.StokvelNotFound(stokvelId);

        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
            return new AddMemberResult.UserNotFound(userId);

        if (stokvel.MemberIds.Contains(userId))
            return new AddMemberResult.AlreadyMember(userId);

        if (stokvel.MemberIds.Count >= 20)
            return new AddMemberResult.MembershipLimitReached();

        stokvel.AddMember(userId);

        await _stokvelRepository.UpdateAsync(stokvel);

        return new AddMemberResult.Added();
    }

    /// <inheritdoc />
    public async Task<bool> RemoveMemberAsync(
        Guid stokvelId,
        Guid userId)
    {
        var stokvel = await _stokvelRepository.GetByIdAsync(stokvelId);

        if (stokvel is null)
            return false;

        if (!stokvel.MemberIds.Contains(userId))
            return false;

        stokvel.RemoveMember(userId);

        await _stokvelRepository.UpdateAsync(stokvel);

        return true;
    }

    /// <inheritdoc />
    public async Task<RecordContributionResult> RecordContributionAsync(
        Guid stokvelId,
        Guid userId,
        string idempotencyKey,
        RecordContributionRequest request)
    {
        var requestHash = CreateRequestHash(
            stokvelId,
            userId,
            request);

        var existingRecord =
            await _idempotencyStore.FindAsync(idempotencyKey);

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

        var stokvel = await _stokvelRepository.GetByIdAsync(stokvelId);

        if (stokvel is null)
        {
            return new RecordContributionResult.StokvelNotFound(
                stokvelId);
        }

        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
        {
            return new RecordContributionResult.UserNotFound(
                userId);
        }

        if (!stokvel.MemberIds.Contains(userId))
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

        try
        {
            var contribution = new Contribution(
                stokvelId,
                userId,
                request.Cycle,
                request.Amount);

            await _contributionRepository.AddAsync(
                contribution);

            var response = contribution.ToResponse();

            await _idempotencyStore.SaveAsync(
                idempotencyKey,
                new IdempotencyRecord(
                    requestHash,
                    response));

            return new RecordContributionResult.Recorded(
                response);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Creates a deterministic SHA-256 hash from the complete
    /// contribution request context.
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