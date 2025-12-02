using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProjectManagement.Api.Core.Application.Authorization;
using ProjectManagement.Api.Core.Domain.Abstractions;
using ProjectManagement.Api.Core.Domain.Enums;
using ProjectManagement.Api.Core.Application.DTOs.Project;
using ProjectManagement.Api.Infrastructure.Extensions;
using ProjectManagement.Api.Core.Application.Mappings;
using ProjectManagement.Api.Infrastructure.Persistence;
using ProjectManagement.Api.Core.Application.Services.Auth;
using ProjectManagement.Api.Core.Application.Services.Links;
using ProjectManagement.Api.Infrastructure.Slices;
using ProjectManagement.Api.Core.Application.Validation.Entities;
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

            RuleFor(c => c.Dto.OwnerId)
                .NotEmpty()
                .When(c => c.Dto.OwnerId.HasValue)
                .WithMessage("Owner ID must be a valid GUID if provided");
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

            Guid ownerId;
            if (command.Dto.OwnerId.HasValue)
            {
                var isAdmin = currentUserService.IsInRole(nameof(UserRole.Admin));
                if (!isAdmin)
                {
                    return Result.Failure<CreateProjectResponse>(Error.Project.CreationForbidden);
                }

                var ownerExists = await dbContext.Users
                    .AnyAsync(u => u.Id == command.Dto.OwnerId.Value, cancellationToken);

                if (!ownerExists)
                {
                    return Result.Failure<CreateProjectResponse>(Error.Project.OwnerNotFound);
                }

                ownerId = command.Dto.OwnerId.Value;
            }
            else
            {
                ownerId = userId.Value;
            }

            var project = command.Dto.ToEntity(ownerId);


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
        Priority Priority,
        Guid? OwnerId = null // Admin can create project for other users
    );

    public sealed record CreateProjectResponse(ProjectDto ProjectDto, string Location);
}
