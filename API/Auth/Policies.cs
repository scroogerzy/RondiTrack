using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using RondiTrack.Domain.Exceptions;
using RondiTrack.Domain.Repositories;

namespace RondiTrack.Auth;

public static class Policies
{
    public const string StokvelMember = "StokvelMember";
    public const string StokvelManager = "StokvelManager";
    public const string RecordContribution = "RecordContribution";
}

public static class StokvelRoles
{
    public const string Member = "Member";
    public const string Treasurer = "Treasurer";
    public const string Chairperson = "Chairperson";
}

public sealed class StokvelAccessRequirement(bool managerRequired = false) : IAuthorizationRequirement
{
    public bool ManagerRequired { get; } = managerRequired;
}

public sealed record ContributionTargetResource(Guid StokvelId, Guid TargetUserId);
public sealed class RecordContributionRequirement : IAuthorizationRequirement;

// Membership and the per-stokvel role are loaded from PostgreSQL for every
// protected operation. They are deliberately not copied into the access token.
public sealed class StokvelAccessHandler(IStokvelMemberRepository members)
    : AuthorizationHandler<StokvelAccessRequirement, Guid>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        StokvelAccessRequirement requirement, Guid stokvelId)
    {
        if (!Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub"), out var userId)) return;

        var membership = await members.GetAsync(userId, stokvelId);
        if (membership is null)
            throw new NotFoundException("Stokvel was not found.");

        if (!requirement.ManagerRequired ||
            membership.Role is StokvelRoles.Treasurer or StokvelRoles.Chairperson)
            context.Succeed(requirement);
    }
}

public sealed class RecordContributionHandler(IStokvelMemberRepository members)
    : AuthorizationHandler<RecordContributionRequirement, ContributionTargetResource>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        RecordContributionRequirement requirement, ContributionTargetResource target)
    {
        if (!Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub"), out var callerId)) return;

        var membership = await members.GetAsync(callerId, target.StokvelId);
        if (membership is null) throw new NotFoundException("Stokvel was not found.");

        if (callerId == target.TargetUserId ||
            membership.Role is StokvelRoles.Treasurer or StokvelRoles.Chairperson)
            context.Succeed(requirement);
    }
}
