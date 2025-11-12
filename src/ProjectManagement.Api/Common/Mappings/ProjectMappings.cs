using System.Linq.Expressions;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.DTOs.Project;
using ProjectManagement.Api.Common.Models;
using ProjectManagement.Api.Common.Services.Sorting;
using ProjectManagement.Api.Features.Projects;

namespace ProjectManagement.Api.Common.Mappings;

internal static class ProjectMappings
{
    public static Expression<Func<Project, TDto>> ProjectToProjectDto<TDto>() where TDto : ProjectDto, new()
    {
        return p => new TDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Priority = p.Priority,
            Status = p.Status
        };
    }

    public static TDto ToProjectDto<TDto>(this Project project, List<LinkDto>? links = null)
        where TDto : ProjectDto, new()
    {
        return new TDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Priority = project.Priority,
            Status = project.Status,
            Links = links ?? []
        };
    }

    public static Project ToEntity(this CreateProject.CreateProjectDto project, Guid ownerId)
    {
        return new Project
        {
            Name = project.Name,
            Description = project.Description,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Priority = project.Priority,
            Status = project.Status,
            OwnerId = ownerId
        };
    }

    public static void UpdateFromDto(this Project project, UpdateProject.UpdateProjectDto dto)
    {
        project.Name = dto.Name;
        project.Description = dto.Description;
        project.StartDate = dto.StartDate;
        project.EndDate = dto.EndDate;
        project.Priority = dto.Priority;
        project.Status = dto.Status;
    }

    public static readonly SortMappingDefinition<ProjectDto, Project> ProjectSortMapping = new()
    {
        Mappings =
        [
            new SortMapping(nameof(ProjectDto.Name), nameof(Project.Name)),
            new SortMapping(nameof(ProjectDto.StartDate), nameof(Project.StartDate)),
            new SortMapping(nameof(ProjectDto.EndDate), nameof(Project.EndDate)),
            new SortMapping(nameof(ProjectDto.Status), nameof(Project.Status)),
            new SortMapping(nameof(ProjectDto.Priority), nameof(Project.Priority))
        ]
    };
}
