using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.DTOs.User;
using ProjectManagement.Api.Common.Extensions;
using ProjectManagement.Api.Common.Mappings;
using ProjectManagement.Api.Common.Services.Auth;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Auth;

internal sealed class GetCurrentUser : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet(
                "api/auth/me",
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.SendQueryAsync<GetCurrentUserQuery, Result<UserDto>>(
                        new GetCurrentUserQuery(),
                        cancellationToken);

                    return result.IsSuccess
                        ? Results.Ok(result.Value)
                        : result.ToProblemDetails();
                }
            )
            .WithTags(nameof(Auth))
            .RequireAuthorization();
    }

    internal sealed record GetCurrentUserQuery : IQuery<Result<UserDto>>;

    internal sealed class GetCurrentUserQueryHandler(
        UserManager<User> userManager,
        ICurrentUserService currentUserService)
        : IQueryHandler<GetCurrentUserQuery, Result<UserDto>>
    {
        public async Task<Result<UserDto>> HandleAsync(GetCurrentUserQuery query,
            CancellationToken cancellationToken = default)
        {
            if (currentUserService.UserId is null)
            {
                return Result.Failure<UserDto>(Error.Unauthorized);
            }

            var user = await userManager.Users
                .FirstOrDefaultAsync(u => u.Id == currentUserService.UserId.Value, cancellationToken);

            return user?.ToUserDto<UserDto>() ?? Result.Failure<UserDto>(Error.NotFound);
        }
    }
}
