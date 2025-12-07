using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Api.Authorization;

public class ProcurementPermissionRequirement(string permission, string action) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;

    public string Action { get; } = action;
}

public class ProcurementPermissionHandler : AuthorizationHandler<ProcurementPermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProcurementPermissionRequirement requirement)
    {
        if (context.User.IsInRole(Domain.Enums.Roles.Admin))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (context.User.IsInRole(Domain.Enums.Roles.Procurement))
        {
            // If a procurement user has no sub-role claim, treat them as fully privileged.
            if (!context.User.HasClaim(claim => string.Equals(claim.Type, "procurement_role_id", StringComparison.OrdinalIgnoreCase)))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        var claimType = $"procurement_permission:{requirement.Permission}";
        var claim = context.User.FindFirst(claimType);

        if (claim is not null)
        {
            var actions = claim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (actions.Any(action => string.Equals(action, requirement.Action, StringComparison.OrdinalIgnoreCase)))
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
