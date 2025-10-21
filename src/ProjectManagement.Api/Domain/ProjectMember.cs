using ProjectManagement.Api.Domain.Enums;

namespace ProjectManagement.Api.Domain;

public class ProjectMember
{
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public ProjectRole RoleInProject { get; set; }
    public DateTime DateJoined { get; set; }

    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
}