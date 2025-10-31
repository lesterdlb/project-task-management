using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Common.Extensions;
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
                var result = await mediator.SendCommandAsync<DeleteUserCommand, Result>(
                    new DeleteUserCommand(userId),
                    cancellationToken);

                return result.IsSuccess
                    ? Results.NoContent()
                    : result.ToProblemDetails();
            }
        );
    }

    internal sealed record DeleteUserCommand(Guid Id) : ICommand<Result>;

    internal sealed class DeleteUserCommandHandler(ProjectManagementDbContext dbContext)
        : ICommandHandler<DeleteUserCommand, Result>
    {
        public async Task<Result> HandleAsync(DeleteUserCommand command, CancellationToken cancellationToken = default)
        {
            var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Id == command.Id, cancellationToken);

            if (user is null)
            {
                return Result.Failure(Error.NotFound);
            }

            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
