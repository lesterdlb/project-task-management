using Microsoft.AspNetCore.Authorization;

namespace ProjectManagement.Api.Core.Application.Authorization;

public sealed class PermissionAuthorizationHandler(params string[] permissions)
    : AuthorizationHandler<PermissionAuthorizationHandler>, IAuthorizationRequirement
{
    private string[] Permissions { get; } = permissions;

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        PermissionAuthorizationHandler requirement)
    {
        if (context.User.Identity?.IsAuthenticated is not true)
        {
            return Task.CompletedTask;
        }

        var userPermissions = context.User
            .FindAll(Authorization.Permissions.ClaimType)
            .Select(c => c.Value)
            .ToHashSet();

        var hasAllPermissions = requirement.Permissions.All(p => userPermissions.Contains(p));

        if (hasAllPermissions)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
