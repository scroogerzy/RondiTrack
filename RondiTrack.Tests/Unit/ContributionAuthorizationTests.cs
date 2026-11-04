#nullable enable
using System.Linq;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using RondiTrack.Auth;
using RondiTrack.Domain.Exceptions;
using RondiTrack.Domain.Models;
using RondiTrack.Domain.Repositories;

namespace RondiTrack.Tests.Unit;

public class ContributionAuthorizationTests
{
    [Fact]
    public async Task Member_CanRecordOwnContribution()
    {
        var members = new TestStokvelMemberRepository();
        var stokvelId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        await members.AddAsync(new StokvelMember(
            memberId, stokvelId, StokvelRoles.Member, DateTime.UtcNow));

        var authorized = await IsAuthorizedAsync(
            members, memberId,
            new ContributionTargetResource(stokvelId, memberId));

        Assert.True(authorized);
    }

    [Fact]
    public async Task Member_CannotRecordForAnotherMember()
    {
        var members = new TestStokvelMemberRepository();
        var stokvelId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var otherMemberId = Guid.NewGuid();

        await members.AddAsync(new StokvelMember(
            memberId, stokvelId, StokvelRoles.Member, DateTime.UtcNow));

        var authorized = await IsAuthorizedAsync(
            members, memberId,
            new ContributionTargetResource(stokvelId, otherMemberId));

        Assert.False(authorized);
    }

    [Fact]
    public async Task Treasurer_CanRecordForAnotherMemberInOwnStokvel()
    {
        var members = new TestStokvelMemberRepository();
        var stokvelId = Guid.NewGuid();
        var treasurerId = Guid.NewGuid();
        var otherMemberId = Guid.NewGuid();

        await members.AddAsync(new StokvelMember(
            treasurerId, stokvelId, StokvelRoles.Treasurer, DateTime.UtcNow));

        var authorized = await IsAuthorizedAsync(
            members, treasurerId,
            new ContributionTargetResource(stokvelId, otherMemberId));

        Assert.True(authorized);
    }

    [Fact]
    public async Task Chairperson_CanRecordForAnotherMemberInOwnStokvel()
    {
        var members = new TestStokvelMemberRepository();
        var stokvelId = Guid.NewGuid();
        var chairpersonId = Guid.NewGuid();
        var otherMemberId = Guid.NewGuid();

        await members.AddAsync(new StokvelMember(
            chairpersonId, stokvelId, StokvelRoles.Chairperson, DateTime.UtcNow));

        var authorized = await IsAuthorizedAsync(
            members, chairpersonId,
            new ContributionTargetResource(stokvelId, otherMemberId));

        Assert.True(authorized);
    }

    [Fact]
    public async Task Treasurer_CannotRecordForAnotherStokvel()
    {
        var members = new TestStokvelMemberRepository();
        var ownStokvelId = Guid.NewGuid();
        var otherStokvelId = Guid.NewGuid();
        var treasurerId = Guid.NewGuid();
        var otherMemberId = Guid.NewGuid();

        await members.AddAsync(new StokvelMember(
            treasurerId, ownStokvelId, StokvelRoles.Treasurer, DateTime.UtcNow));

        var requirement = new RecordContributionRequirement();

        var context = new AuthorizationHandlerContext(
            new IAuthorizationRequirement[] { requirement },
            CreatePrincipal(treasurerId),
            new ContributionTargetResource(otherStokvelId, otherMemberId));

        var handler = new RecordContributionHandler(members);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(context));
    }

    private static async Task<bool> IsAuthorizedAsync(
        IStokvelMemberRepository members,
        Guid callerId,
        ContributionTargetResource target)
    {
        var requirement = new RecordContributionRequirement();

        var context = new AuthorizationHandlerContext(
            new IAuthorizationRequirement[] { requirement },
            CreatePrincipal(callerId),
            target);

        var handler = new RecordContributionHandler(members);

        await handler.HandleAsync(context);

        return context.HasSucceeded;
    }

    private static ClaimsPrincipal CreatePrincipal(Guid userId)
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            },
            "TestAuthentication");

        return new ClaimsPrincipal(identity);
    }

    private sealed class TestStokvelMemberRepository
        : IStokvelMemberRepository
    {
        private readonly List<StokvelMember> _members = new();

        public Task<IEnumerable<StokvelMember>> GetByStokvelIdAsync(
            Guid stokvelId)
        {
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
            _members.Add(member);
            return Task.FromResult(member);
        }

        public Task<bool> DeleteAsync(Guid userId, Guid stokvelId)
        {
            var member = _members.FirstOrDefault(
                item => item.UserId == userId &&
                        item.StokvelId == stokvelId);

            if (member is null)

                return Task.FromResult(false);

            _members.Remove(member);
            return Task.FromResult(true);
        }
    }
}
