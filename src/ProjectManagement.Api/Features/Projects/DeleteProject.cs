using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Common.Authorization;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Common.Extensions;
using ProjectManagement.Api.Common.Persistence;
using ProjectManagement.Api.Common.Services.Auth;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Constants;
using ProjectManagement.Api.Features.Users;
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

            var project = await dbContext.Projects.SingleOrDefaultAsync(p => p.Id == command.Id && p.OwnerId == userId,
                cancellationToken);
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
