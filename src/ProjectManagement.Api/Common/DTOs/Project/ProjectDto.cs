using ProjectManagement.Api.Common.Domain.Enums;

namespace ProjectManagement.Api.Common.DTOs.Project;

public class ProjectDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public ProjectStatus Status { get; init; }
    public Priority Priority { get; init; }
}
