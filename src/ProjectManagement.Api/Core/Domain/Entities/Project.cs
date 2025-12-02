using ProjectManagement.Api.Core.Domain.Enums;

namespace ProjectManagement.Api.Core.Domain.Entities;

public sealed class Project : Entity
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ProjectStatus Status { get; set; }
    public Priority Priority { get; set; }

    public Guid OwnerId { get; init; }
    public User Owner { get; init; }
    public ICollection<ProjectMember> Members { get; init; } = [];
    public ICollection<ProjectTask> Tasks { get; init; } = [];
    public ICollection<Label> Labels { get; init; } = [];
}
