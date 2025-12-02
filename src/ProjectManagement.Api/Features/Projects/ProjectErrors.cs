using ProjectManagement.Api.Core.Domain.Abstractions;

namespace ProjectManagement.Api.Features.Projects;

public static class ProjectErrors
{
    public static Error CreationForbidden =>
        new("Project.CreationForbidden",
            "Only administrators can create projects for other users");

    public static Error OwnerNotFound =>
        new("Project.OwnerNotFound",
            "The specified project owner does not exist",
            ErrorCategory.NotFound);
}
