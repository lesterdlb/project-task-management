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
            RuleFor(c => c.Dto.Name)
                .NotEmpty()
                .WithMessage("Project name is required.")
                .MaximumLength(200)
                .WithMessage("Project name must not exceed 200 characters.");
            RuleFor(c => c.Dto.Description)
                .MaximumLength(5000)
                .WithMessage("Description must not exceed 5000 characters.");
            RuleFor(c => c.Dto.StartDate)
                .NotEmpty()
                .GreaterThanOrEqualTo(DateTime.Now)
                .WithMessage("Start date is required and must be in the future.");
            RuleFor(c => c.Dto.EndDate)
                .GreaterThan(c => c.Dto.StartDate)
                .WithMessage("End date must be after start date.");
            RuleFor(c => c.Dto.Status)
                .IsInEnum()
                .WithMessage("Status is required and must be a valid value.");
            RuleFor(c => c.Dto.Priority)
                .IsInEnum()
                .WithMessage("Priority is required and must be a valid value.");
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

            var projectDto = project.ToProjectDto<ProjectDto>();

            projectDto.Links = linkService.CreateLinksForItem(
                EndpointNames.Projects.Names.GetProject,
                EndpointNames.Projects.Names.UpdateProject,
                EndpointNames.Projects.Names.DeleteProject,
                projectDto.Id);

            return new CreateProjectResponse
            {
                ProjectDto = projectDto,
                Location = linkService.CreateHref(
                    EndpointNames.Projects.Names.GetProject,
                    new { id = project.Id })
            };
        }
    }

    public sealed class CreateProjectDto
    {
        public string Name { get; init; }
        public string Description { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public ProjectStatus Status { get; init; }
        public Priority Priority { get; init; }
    }

    public sealed class CreateProjectResponse
    {
        public ProjectDto ProjectDto { get; init; }
        public string Location { get; init; }
    }
}
