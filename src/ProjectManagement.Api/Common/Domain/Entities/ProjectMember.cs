using ProjectManagement.Api.Common.Domain.Enums;

namespace ProjectManagement.Api.Common.Domain.Entities;

public sealed class ProjectMember
{
    public ProjectRole RoleInProject { get; init; }
    public DateTime DateJoined { get; init; }
    
    public Guid ProjectId { get; init; }
    public Project Project { get; init; }
    public Guid UserId { get; init; }
    public User User { get; init; }
}
