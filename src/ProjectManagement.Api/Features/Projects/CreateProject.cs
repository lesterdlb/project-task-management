using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProjectManagement.Api.Common.Authorization;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Common.Domain.Enums;
using ProjectManagement.Api.Common.DTOs.Project;
using ProjectManagement.Api.Common.Extensions;
using ProjectManagement.Api.Common.Mappings;
using ProjectManagement.Api.Common.Persistence;
using ProjectManagement.Api.Common.Services.Auth;
using ProjectManagement.Api.Common.Services.Links;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Common.Validators;
using ProjectManagement.Api.Constants;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Projects;

internal sealed class CreateProject : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost(
                EndpointNames.Projects.Routes.Base,
                async (
                    CreateProjectDto createProjectDto,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.SendCommandAsync<CreateProjectCommand, Result<CreateProjectResponse>>(
                        new CreateProjectCommand(createProjectDto),
                        cancellationToken);

                    return result.IsSuccess
                        ? Results.Created(result.Value.Location, result.Value.ProjectDto)
                        : result.ToProblemDetails();
                }
            )
            .WithName(EndpointNames.Projects.Names.CreateProject)
            .WithTags(EndpointNames.Projects.GroupName)
            .RequirePermissions(Permissions.Projects.Write);
    }

    internal sealed record CreateProjectCommand(CreateProjectDto Dto) : ICommand<Result<CreateProjectResponse>>;

    internal sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
    {
        public CreateProjectCommandValidator()
        {
            RuleFor(c => c.Dto.Name).ValidateProjectName();
            RuleFor(c => c.Dto.Description).ValidateProjectDescription();
            RuleFor(c => c.Dto.StartDate).ValidateProjectStartDate();
            RuleFor(c => c.Dto.EndDate).ValidateProjectEndDate(c => c.Dto.StartDate);
            RuleFor(c => c.Dto.Status).ValidateProjectStatus();
            RuleFor(c => c.Dto.Priority).ValidateProjectPriority();
        }
    }

    internal sealed class CreateProjectCommandHandler(
        ProjectManagementDbContext dbContext,
        ICurrentUserService currentUserService,
        ILinkService linkService)
        : ICommandHandler<CreateProjectCommand, Result<CreateProjectResponse>>
    {
        public async Task<Result<CreateProjectResponse>> HandleAsync(CreateProjectCommand command,
            CancellationToken cancellationToken = default)
        {
            var userId = currentUserService.UserId;

            if (userId is null)
            {
                return Result.Failure<CreateProjectResponse>(Error.Unauthorized);
            }

            var project = command.Dto.ToEntity(userId.Value);

            try
            {
                dbContext.Projects.Add(project);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
            {
                if (pgEx.ConstraintName?.Contains("name") is true)
                {
                    return Result.Failure<CreateProjectResponse>(Error.Conflict);
                }

                throw;
            }

            var projectDto = project.ToProjectDto<ProjectDto>(
                linkService.CreateLinksForItem(
                    EndpointNames.Projects.Names.GetProject,
                    EndpointNames.Projects.Names.UpdateProject,
                    EndpointNames.Projects.Names.DeleteProject,
                    project.Id)
            );

            return new CreateProjectResponse(
                projectDto,
                linkService.CreateHref(EndpointNames.Projects.Names.GetProject, new { id = project.Id })
            );
        }
    }

    public sealed record CreateProjectDto(
        string Name,
        string Description,
        DateTime StartDate,
        DateTime? EndDate,
        ProjectStatus Status,
        Priority Priority
    );

    public sealed record CreateProjectResponse(ProjectDto ProjectDto, string Location);
}
