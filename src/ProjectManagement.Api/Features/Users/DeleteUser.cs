using Microsoft.AspNetCore.Identity;
using ProjectManagement.Api.Common.Authorization;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.Extensions;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Users;

internal sealed class DeleteUser : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete(
                "api/users/{userId:guid}",
                async (
                    Guid userId,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.SendCommandAsync<DeleteUserCommand, Result>(
                        new DeleteUserCommand(userId),
                        cancellationToken);

                    return result.IsSuccess
                        ? Results.NoContent()
                        : result.ToProblemDetails();
                }
            )
            .WithTags(nameof(Users))
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
