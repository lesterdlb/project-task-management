using Microsoft.AspNetCore.Identity;
using ProjectManagement.Api.Core.Domain.Enums;

namespace ProjectManagement.Api.Core.Domain.Entities;

public sealed class User : IdentityUser<Guid>
{
    public required string FullName { get; set; }
    public string? AvatarUrl { get; init; }
    public UserRole Role { get; init; }

    public Guid CreatedBy { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public uint Version { get; set; }

    public ICollection<Project> OwnedProjects { get; init; } = [];
    public ICollection<ProjectMember> ProjectMemberships { get; init; } = [];
    public ICollection<ProjectTask> AssignedTasks { get; init; } = [];
    public ICollection<ProjectTask> CreatedTasks { get; init; } = [];
    public ICollection<Comment> Comments { get; init; } = [];
}
