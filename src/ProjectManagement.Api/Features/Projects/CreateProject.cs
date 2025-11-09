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
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Projects;

internal sealed class CreateProject : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost(
                "api/projects",
                async (
                    CreateProjectDto createProjectDto,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.SendCommandAsync<CreateProjectCommand, Result<ProjectDto>>(
                        new CreateProjectCommand(createProjectDto),
                        cancellationToken);

                    return result.IsSuccess
                        ? Results.Created($"api/projects/{result.Value.Id}", result.Value)
                        : result.ToProblemDetails();
                }
            )
            .WithTags(nameof(Projects))
            .RequirePermissions(Permissions.Projects.Write);
    }

    internal sealed record CreateProjectCommand(CreateProjectDto Dto) : ICommand<Result<ProjectDto>>;

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
        ICurrentUserService currentUserService)
        : ICommandHandler<CreateProjectCommand, Result<ProjectDto>>
    {
        public async Task<Result<ProjectDto>> HandleAsync(CreateProjectCommand command,
            CancellationToken cancellationToken = default)
        {
            var userId = currentUserService.UserId;

            if (userId is null)
            {
                return Result.Failure<ProjectDto>(Error.Unauthorized);
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
                    return Result.Failure<ProjectDto>(Error.Conflict);
                }

                throw;
            }

            return project.ToProjectDto<ProjectDto>();
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
}
