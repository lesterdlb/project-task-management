using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Common.Persistence;
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
                var deleted = await mediator.SendCommandAsync<DeleteUserCommand, bool>(
                    new DeleteUserCommand(userId),
                    cancellationToken);

                return deleted ? Results.NoContent() : Results.NotFound();
            }
        );
    }

    internal sealed record DeleteUserCommand(Guid Id) : ICommand<bool>;

    internal sealed class DeleteUserCommandHandler(ProjectManagementDbContext dbContext)
        : ICommandHandler<DeleteUserCommand, bool>
    {
        public async Task<bool> HandleAsync(DeleteUserCommand command, CancellationToken cancellationToken = default)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == command.Id, cancellationToken);

            if (user is null)
            {
                return false;
            }

            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
