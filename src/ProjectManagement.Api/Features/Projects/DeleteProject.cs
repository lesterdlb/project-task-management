using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Core.Application.Authorization;
using ProjectManagement.Api.Core.Domain.Abstractions;
using ProjectManagement.Api.Core.Domain.Enums;
using ProjectManagement.Api.Infrastructure.Extensions;
using ProjectManagement.Api.Infrastructure.Persistence;
using ProjectManagement.Api.Core.Application.Services.Auth;
using ProjectManagement.Api.Infrastructure.Slices;
using ProjectManagement.Api.Constants;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Projects;

internal sealed class DeleteProject : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete(
                EndpointNames.Projects.Routes.ById,
                async (
                    Guid id,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.SendCommandAsync<DeleteProjectCommand, Result>(
                        new DeleteProjectCommand(id),
                        cancellationToken);

                    return result.IsSuccess
                        ? Results.NoContent()
                        : result.ToProblemDetails();
                }
            )
            .WithName(EndpointNames.Projects.Names.DeleteProject)
            .WithTags(EndpointNames.Projects.GroupName)
            .RequirePermissions(Permissions.Projects.Delete);
    }

    internal sealed record DeleteProjectCommand(Guid Id) : ICommand<Result>;

    internal sealed class DeleteProjectCommandHandler(
        ProjectManagementDbContext dbContext,
        ICurrentUserService currentUserService)
        : ICommandHandler<DeleteProjectCommand, Result>
    {
        public async Task<Result> HandleAsync(DeleteProjectCommand command,
            CancellationToken cancellationToken = default)
        {
            var userId = currentUserService.UserId;
            if (userId is null)
            {
                return Result.Failure(Error.Unauthorized);
            }

            var isAdmin = currentUserService.IsInRole(nameof(UserRole.Admin));

            var project = await dbContext.Projects
                .Where(p => p.Id == command.Id &&
                            (isAdmin || p.OwnerId == userId))
                .SingleOrDefaultAsync(cancellationToken);

            if (project is null)
            {
                return Result.Failure(Error.NotFound);
            }

            dbContext.Projects.Remove(project);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
