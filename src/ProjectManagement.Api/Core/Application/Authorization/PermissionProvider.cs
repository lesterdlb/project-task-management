using ProjectManagement.Api.Core.Domain.Enums;

namespace ProjectManagement.Api.Core.Application.Authorization;

public static class PermissionProvider
{
    public static IEnumerable<string> GetPermissionsForRole(UserRole role)
    {
        return role switch
        {
            UserRole.Guest => GetGuestPermissions(),
            UserRole.Member => GetMemberPermissions(),
            UserRole.Admin => GetAdminPermissions(),
            _ => []
        };
    }

    private static IEnumerable<string> GetGuestPermissions()
    {
        return []; // Guests can't do anything
    }

    private static List<string> GetMemberPermissions()
    {
        var permissions = new List<string>();

        permissions.AddRange(GetGuestPermissions()); // Inherit all Guest permissions

        permissions.AddRange([
            Permissions.Users.Read,
            Permissions.Projects.Read,
            Permissions.Projects.Write,
            Permissions.Projects.Delete,
            Permissions.Tasks.Read,
            Permissions.Tasks.Write,
            Permissions.Tasks.Delete,
            Permissions.Labels.Read,
            Permissions.Labels.Write,
            Permissions.Labels.Delete,
            Permissions.Comments.Read,
            Permissions.Comments.Write,
            Permissions.Comments.Delete
        ]);

        return permissions;
    }

    private static List<string> GetAdminPermissions()
    {
        var permissions = new List<string>();

        permissions.AddRange(GetMemberPermissions()); // Inherit all Member permissions

        permissions.AddRange([
            Permissions.Users.Read,
            Permissions.Users.Write,
            Permissions.Users.Delete,
        ]);

        return permissions;
    }
}
