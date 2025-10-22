using ProjectManagement.Api.Common.Domain.Enums;

namespace ProjectManagement.Api.Common.Domain.Entities;

public sealed class User : Entity
{
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required string FullName { get; init; }
    public string? AvatarUrl { get; init; }
    public UserRole Role { get; init; }

    public ICollection<Project> OwnedProjects { get; init; } = [];
    public ICollection<ProjectMember> ProjectMemberships { get; init; } = [];
    public ICollection<ProjectTask> AssignedTasks { get; init; } = [];
    public ICollection<ProjectTask> CreatedTasks { get; init; } = [];
    public ICollection<Comment> Comments { get; init; } = [];
}