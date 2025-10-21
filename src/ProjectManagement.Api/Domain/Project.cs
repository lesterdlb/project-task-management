using ProjectManagement.Api.Domain.Entities;
using ProjectManagement.Api.Domain.Enums;

namespace ProjectManagement.Api.Domain;

public class Project : Entity
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ProjectStatus Status { get; set; }
    public Priority Priority { get; set; }
    public Guid OwnerId { get; set; }

    public User Owner { get; set; } = null!;
    public ICollection<ProjectMember> Members { get; set; } = [];
    public ICollection<ProjectTask> Tasks { get; set; } = [];
    public ICollection<Label> Labels { get; set; } = [];
}