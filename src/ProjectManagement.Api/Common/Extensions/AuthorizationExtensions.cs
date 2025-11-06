using ProjectManagement.Api.Common.Authorization;

namespace ProjectManagement.Api.Common.Extensions;

public static class AuthorizationExtensions
{
    public static RouteHandlerBuilder RequirePermissions(this RouteHandlerBuilder builder, params string[] permissions)
    {
        return builder.RequireAuthorization(policy =>
            policy.AddRequirements(new PermissionAuthorizationHandler(permissions)));
    }
}
