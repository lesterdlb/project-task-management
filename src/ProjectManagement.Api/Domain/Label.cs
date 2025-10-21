using ProjectManagement.Api.Domain.Entities;

namespace ProjectManagement.Api.Domain;

public class Label : Entity
{
    public required string Name { get; set; }
    public string Color { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;
    public ICollection<ProjectTaskLabel> TaskLabels { get; set; } = [];
}