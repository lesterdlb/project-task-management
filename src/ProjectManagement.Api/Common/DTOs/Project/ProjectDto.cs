using ProjectManagement.Api.Common.Domain.Enums;
using ProjectManagement.Api.Common.Models;

namespace ProjectManagement.Api.Common.DTOs.Project;

public class ProjectDto : ILinksResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public ProjectStatus Status { get; init; }
    public Priority Priority { get; init; }

    public List<LinkDto> Links { get; set; }
}
