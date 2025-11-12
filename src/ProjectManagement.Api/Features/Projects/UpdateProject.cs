using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProjectManagement.Api.Common.Authorization;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Common.Domain.Enums;
using ProjectManagement.Api.Common.Extensions;
using ProjectManagement.Api.Common.Mappings;
using ProjectManagement.Api.Common.Persistence;
using ProjectManagement.Api.Common.Services.Auth;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Common.Validators;
using ProjectManagement.Api.Constants;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Projects;

internal sealed class UpdateProject : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut(
                EndpointNames.Projects.Routes.ById,
                async (
                    Guid id,
                    UpdateProjectDto updateProjectDto,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.SendCommandAsync<UpdateProjectCommand, Result>(
                        new UpdateProjectCommand(id, updateProjectDto),
                        cancellationToken);

                    return result.IsSuccess
                        ? Results.NoContent()
                        : result.ToProblemDetails();
                }
            )
            .WithName(EndpointNames.Projects.Names.UpdateProject)
            .WithTags(EndpointNames.Projects.GroupName)
            .RequirePermissions(Permissions.Projects.Write);
    }

    internal sealed record UpdateProjectCommand(Guid Id, UpdateProjectDto Dto) : ICommand<Result>;

    internal sealed class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
    {
        public UpdateProjectCommandValidator()
        {
            RuleFor(c => c.Dto.Name).ValidateProjectName();
            RuleFor(c => c.Dto.Description).ValidateProjectDescription();
            RuleFor(c => c.Dto.StartDate).ValidateProjectStartDate();
            RuleFor(c => c.Dto.EndDate).ValidateProjectEndDate(c => c.Dto.StartDate);
            RuleFor(c => c.Dto.Status).ValidateProjectStatus();
            RuleFor(c => c.Dto.Priority).ValidateProjectPriority();
        }
    }

    internal sealed class UpdateProjectCommandHandler(
        ProjectManagementDbContext dbContext,
        ICurrentUserService currentUserService)
        : ICommandHandler<UpdateProjectCommand, Result>
    {
        public async Task<Result> HandleAsync(UpdateProjectCommand command,
            CancellationToken cancellationToken = default)
        {
            var userId = currentUserService.UserId;
            if (userId is null)
            {
                return Result.Failure(Error.Unauthorized);
            }

            var project = await dbContext.Projects
                .Where(p => p.Id == command.Id && p.OwnerId == userId)
                .SingleOrDefaultAsync(cancellationToken);

            if (project is null)
            {
                return Result.Failure(Error.NotFound);
            }

            project.UpdateFromDto(command.Dto);

            try
            {
                dbContext.Update(project);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
            {
                if (pgEx.ConstraintName?.Contains("name") is true)
                {
                    return Result.Failure(Error.Conflict);
                }

                throw;
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Failure(Error.Conflict);
            }

            return Result.Success();
        }
    }

    public sealed record UpdateProjectDto(
        string Name,
        string Description,
        DateTime StartDate,
        DateTime? EndDate,
        ProjectStatus Status,
        Priority Priority
    );
}
