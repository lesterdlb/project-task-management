using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Core.Application.Authorization;
using ProjectManagement.Api.Core.Domain.Abstractions;
using ProjectManagement.Api.Core.Domain.Entities;
using ProjectManagement.Api.Core.Domain.Enums;
using ProjectManagement.Api.Infrastructure.Extensions;
using ProjectManagement.Api.Infrastructure.Persistence;
using ProjectManagement.Api.Core.Application.Services.Auth;
using ProjectManagement.Api.Infrastructure.Slices;
using ProjectManagement.Api.Constants;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Projects;

internal sealed class AddProjectMember : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost(
                $"{EndpointNames.Projects.Routes.ById}/members",
                async (
                    Guid id,
                    AddProjectMemberDto dto,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.SendCommandAsync<AddProjectMemberCommand, Result>(
                        new AddProjectMemberCommand(id, dto),
                        cancellationToken);

                    return result.IsSuccess
                        ? Results.NoContent()
                        : result.ToProblemDetails();
                }
            )
            .WithName(EndpointNames.Projects.Names.AddProjectMember)
            .WithTags(EndpointNames.Projects.GroupName)
            .RequirePermissions(Permissions.Projects.Write);
    }

    internal sealed record AddProjectMemberCommand(Guid ProjectId, AddProjectMemberDto Dto) : ICommand<Result>;

    internal sealed class AddProjectMemberCommandValidator : AbstractValidator<AddProjectMemberCommand>
    {
        public AddProjectMemberCommandValidator()
        {
            RuleFor(c => c.ProjectId).NotEmpty();
            RuleFor(c => c.Dto.UserId).NotEmpty();
            RuleFor(c => c.Dto.Role).IsInEnum();
        }
    }

    internal sealed class AddProjectMemberCommandHandler(
        ProjectManagementDbContext dbContext,
        ICurrentUserService currentUserService)
        : ICommandHandler<AddProjectMemberCommand, Result>
    {
        public async Task<Result> HandleAsync(AddProjectMemberCommand command,
            CancellationToken cancellationToken = default)
        {
            var userId = currentUserService.UserId;
            if (userId is null)
            {
                return Result.Failure(Error.Unauthorized);
            }

            var isAdmin = currentUserService.IsInRole(nameof(UserRole.Admin));

            var project = await dbContext.Projects
                .Include(p => p.Members)
                .Where(p => p.Id == command.ProjectId &&
                            (isAdmin || p.OwnerId == userId))
                .SingleOrDefaultAsync(cancellationToken);

            if (project is null)
            {
                return Result.Failure(Error.NotFound);
            }

            var userExists = await dbContext.Users.AnyAsync(u => u.Id == command.Dto.UserId, cancellationToken);

            if (!userExists)
            {
                return Result.Failure(ProjectMemberErrors.UserNotFound);
            }

            if (project.Members.Any(m => m.UserId == command.Dto.UserId))
            {
                return Result.Failure(ProjectMemberErrors.AlreadyMember);
            }

            if (project.OwnerId == command.Dto.UserId)
            {
                return Result.Failure(ProjectMemberErrors.OwnerAsMember);
            }

            var projectMember = new ProjectMember
            {
                ProjectId = command.ProjectId,
                UserId = command.Dto.UserId,
                RoleInProject = ProjectRole.Contributor,
                DateJoined = DateTime.UtcNow
            };

            dbContext.ProjectMembers.Add(projectMember);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public sealed record AddProjectMemberDto(
        Guid UserId,
        ProjectRole Role = ProjectRole.Contributor
    );
}
