using ProjectManagement.Api.Core.Domain.Enums;

namespace ProjectManagement.Api.Core.Domain.Entities;

public sealed class ProjectMember
{
    public ProjectRole RoleInProject { get; init; }
    public DateTime DateJoined { get; init; }
    
    public Guid ProjectId { get; init; }
    public Project Project { get; init; }
    public Guid UserId { get; init; }
    public User User { get; init; }
}
