using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Core.Domain.Abstractions;
using ProjectManagement.Api.Core.Domain.Entities;
using ProjectManagement.Api.Core.Application.DTOs.User;
using ProjectManagement.Api.Infrastructure.Extensions;
using ProjectManagement.Api.Core.Application.Mappings;
using ProjectManagement.Api.Core.Application.Services.Auth;
using ProjectManagement.Api.Infrastructure.Slices;
using ProjectManagement.Api.Constants;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Auth;

internal sealed class GetCurrentUser : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet(
                EndpointNames.Auth.Routes.GetCurrentUser,
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
            .WithName(EndpointNames.Auth.Names.GetCurrentUser)
            .WithTags(EndpointNames.Auth.GroupName)
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
