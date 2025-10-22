using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Common.Persistence;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Users;

public sealed class GetUsers : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet(
            "api/users",
            async (IMediator mediator, CancellationToken cancellationToken) =>
            {
                var query = new GetUsersQuery();
                return await mediator.SendQueryAsync<GetUsersQuery, IEnumerable<UserDto>>(query, cancellationToken);
            }
        );
    }

    public sealed record GetUsersQuery : IQuery<IEnumerable<UserDto>>;

    public class GetUsersQueryHandler(ProjectManagementDbContext dbContext)
        : IQueryHandler<GetUsersQuery, IEnumerable<UserDto>>
    {
        public async Task<IEnumerable<UserDto>> HandleAsync(
            GetUsersQuery query,
            CancellationToken cancellationToken = default)
        {
            var users = await dbContext.Users.ToListAsync(cancellationToken);
            return users.Select(u => new UserDto
            {
                UserName = u.UserName,
                Email = u.Email,
                FullName = u.FullName
            });
        }
    }

    public sealed class UserDto
    {
        public required string UserName { get; init; }
        public required string Email { get; init; }
        public required string FullName { get; init; }
    }
}
