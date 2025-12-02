using ProjectManagement.Api.Core.Domain.Abstractions;

namespace ProjectManagement.Api.Features.Projects;

public static class ProjectMemberErrors
{
    public static Error UserNotFound =>
        new("ProjectMember.UserNotFound",
            "The specified user does not exist",
            ErrorCategory.NotFound);

    public static Error AlreadyMember =>
        new("ProjectMember.AlreadyMember",
            "User is already a member of this project");

    public static Error OwnerAsMember =>
        new("ProjectMember.OwnerAsMember",
            "Project owner cannot be added as a member");

    public static Error NotFound =>
        new("ProjectMember.NotFound",
            "Project member not found",
            ErrorCategory.NotFound);

    public static Error CannotRemoveOwner =>
        new("ProjectMember.CannotRemoveOwner",
            "Cannot remove project owner from project");
}
