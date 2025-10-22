using ProjectManagement.Api.Common.Domain.Enums;

namespace ProjectManagement.Api.Common.Domain.Entities;

public sealed class ProjectTask : Entity
{
    public required string Title { get; init; }
    public string Description { get; init; } = string.Empty;
    public ProjectTaskStatus Status { get; init; }
    public Priority Priority { get; init; }
    public DateTime? DueDate { get; init; }

    public Guid ProjectId { get; init; }
    public required Project Project { get; init; }
    public Guid? AssignedToId { get; init; }
    public User? AssignedTo { get; init; }
    public ICollection<Comment> Comments { get; init; } = [];
}