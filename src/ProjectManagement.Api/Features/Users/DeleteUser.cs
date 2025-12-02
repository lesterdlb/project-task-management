using Microsoft.AspNetCore.Identity;
using ProjectManagement.Api.Core.Application.Authorization;
using ProjectManagement.Api.Core.Domain.Abstractions;
using ProjectManagement.Api.Core.Domain.Entities;
using ProjectManagement.Api.Infrastructure.Extensions;
using ProjectManagement.Api.Infrastructure.Slices;
using ProjectManagement.Api.Constants;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Users;

internal sealed class DeleteUser : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete(
                EndpointNames.Users.Routes.ById,
                async (
                    Guid id,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.SendCommandAsync<DeleteUserCommand, Result>(
                        new DeleteUserCommand(id),
                        cancellationToken);

                    return result.IsSuccess
                        ? Results.NoContent()
                        : result.ToProblemDetails();
                }
            )
            .WithName(EndpointNames.Users.Names.DeleteUser)
            .WithTags(EndpointNames.Users.GroupName)
            .RequirePermissions(Permissions.Users.Delete);
    }

    internal sealed record DeleteUserCommand(Guid Id) : ICommand<Result>;

    internal sealed class DeleteUserCommandHandler(UserManager<User> userManager)
        : ICommandHandler<DeleteUserCommand, Result>
    {
        public async Task<Result> HandleAsync(DeleteUserCommand command, CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByIdAsync(command.Id.ToString());
            if (user is null)
            {
                return Result.Failure(Error.NotFound);
            }

            await userManager.DeleteAsync(user);

            return Result.Success();
        }
    }
}
