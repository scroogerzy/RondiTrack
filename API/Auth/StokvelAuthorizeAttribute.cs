using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RondiTrack.Auth;

// Invokes the named resource policy using the stokvel id from the route.
// The policy and its handler own the actual membership/role decision.
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class StokvelAuthorizeAttribute : TypeFilterAttribute
{
    public StokvelAuthorizeAttribute(string policy) : base(typeof(StokvelAuthorizeFilter))
    { Arguments = new object[] { policy }; }
}

public sealed class StokvelAuthorizeFilter(string policy, IAuthorizationService authorization)
    : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var route = context.RouteData.Values;
        var idValue = route.TryGetValue("stokvelId", out var stokvelIdValue)
            ? stokvelIdValue : route.TryGetValue("id", out var id) ? id : null;
        if (!Guid.TryParse(idValue?.ToString(), out var stokvelId))
        {
            context.Result = new ForbidResult();
            return;
        }

        object resource = policy == Policies.RecordContribution
            ? new ContributionTargetResource(stokvelId, Guid.TryParse(route["userId"]?.ToString(), out var targetId) ? targetId : Guid.Empty)
            : stokvelId;
        var result = await authorization.AuthorizeAsync(context.HttpContext.User, resource, policy);
        if (!result.Succeeded) context.Result = new ForbidResult();
    }
}
