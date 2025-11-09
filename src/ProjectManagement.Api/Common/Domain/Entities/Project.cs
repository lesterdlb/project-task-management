using ProjectManagement.Api.Common.Domain.Enums;

namespace ProjectManagement.Api.Common.Domain.Entities;

public sealed class Project : Entity
{
    public required string Name { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public ProjectStatus Status { get; init; }
    public Priority Priority { get; init; }

    public Guid OwnerId { get; init; }
    public User Owner { get; init; }
    public ICollection<ProjectMember> Members { get; init; } = [];
    public ICollection<ProjectTask> Tasks { get; init; } = [];
    public ICollection<Label> Labels { get; init; } = [];
}
