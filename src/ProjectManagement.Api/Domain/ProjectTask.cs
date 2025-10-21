using ProjectManagement.Api.Domain.Entities;
using ProjectManagement.Api.Domain.Enums;

namespace ProjectManagement.Api.Domain;

public class ProjectTask : Entity
{
    public Guid ProjectId { get; set; }
    public required string Title { get; set; }
    public string Description { get; set; } = string.Empty;
    public ProjectTaskStatus Status { get; set; }
    public Priority Priority { get; set; }
    public Guid? AssignedToId { get; set; }
    public DateTime? DueDate { get; set; }

    public Project Project { get; set; } = null!;
    public User? AssignedTo { get; set; }
    public ICollection<ProjectTaskLabel> TaskLabels { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
}