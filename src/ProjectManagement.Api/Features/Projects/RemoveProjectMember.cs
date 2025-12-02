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

internal sealed class RemoveProjectMember : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete(
                $"{EndpointNames.Projects.Routes.ById}/members/{{userId:guid}}",
                async (
                    Guid id,
                    Guid userId,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.SendCommandAsync<RemoveProjectMemberCommand, Result>(
                        new RemoveProjectMemberCommand(id, userId),
                        cancellationToken);

                    return result.IsSuccess
                        ? Results.NoContent()
                        : result.ToProblemDetails();
                }
            )
            .WithName(EndpointNames.Projects.Names.RemoveProjectMember)
            .WithTags(EndpointNames.Projects.GroupName)
            .RequirePermissions(Permissions.Projects.Write);
    }

    internal sealed record RemoveProjectMemberCommand(Guid ProjectId, Guid UserId) : ICommand<Result>;

    internal sealed class RemoveProjectMemberCommandHandler(
        ProjectManagementDbContext dbContext,
        ICurrentUserService currentUserService)
        : ICommandHandler<RemoveProjectMemberCommand, Result>
    {
        public async Task<Result> HandleAsync(RemoveProjectMemberCommand command,
            CancellationToken cancellationToken = default)
        {
            var userId = currentUserService.UserId;
            if (userId is null)
            {
                return Result.Failure(Error.Unauthorized);
            }

            var isAdmin = currentUserService.IsInRole(nameof(UserRole.Admin));

            var project = await dbContext.Projects
                .Where(p => p.Id == command.ProjectId &&
                            (isAdmin || p.OwnerId == userId))
                .SingleOrDefaultAsync(cancellationToken);

            if (project is null)
            {
                return Result.Failure(Error.NotFound);
            }

            var projectMember = await dbContext.ProjectMembers
                .Where(pm => pm.ProjectId == command.ProjectId && pm.UserId == command.UserId)
                .SingleOrDefaultAsync(cancellationToken);

            if (projectMember is null)
            {
                return Result.Failure(ProjectMemberErrors.UserNotFound);
            }

            dbContext.ProjectMembers.Remove(projectMember);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
