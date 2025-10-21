using ProjectManagement.Api.Domain.Entities;
using ProjectManagement.Api.Domain.Enums;

namespace ProjectManagement.Api.Domain;

public class User : Entity
{
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; }

    public ICollection<Project> OwnedProjects { get; set; } = [];
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = [];
    public ICollection<ProjectTask> AssignedTasks { get; set; } = [];
    public ICollection<ProjectTask> CreatedTasks { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
}