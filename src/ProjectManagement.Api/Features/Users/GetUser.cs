using System.Dynamic;
using System.Linq.Expressions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.Filters;
using ProjectManagement.Api.Common.Models;
using ProjectManagement.Api.Common.Persistence;
using ProjectManagement.Api.Common.Services;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Users;

internal sealed class GetUser : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet(
                "api/users/{userId:guid}",
                async (
                    Guid userId,
                    [AsParameters] UserQueryParameters query,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var user = await mediator.SendQueryAsync<GetUserQuery, ExpandoObject?>(
                        new GetUserQuery(userId, query),
                        cancellationToken);

                    return user is null ? Results.NotFound() : Results.Ok(user);
                }
            )
            .AddEndpointFilter<ValidationFilter<UserQueryParameters>>();
    }

    internal sealed record GetUserQuery(Guid Id, BaseQueryParameters Parameters) : IQuery<ExpandoObject?>;

    internal sealed class GetUserQueryHandler(
        ProjectManagementDbContext dbContext,
        DataShapingService dataShapingService
    )
        : IQueryHandler<GetUserQuery, ExpandoObject?>
    {
        public async Task<ExpandoObject?> HandleAsync(GetUserQuery query, CancellationToken cancellationToken = default)
        {
            var user = await dbContext
                .Users
                .Where(u => u.Id == query.Id)
                .Select(UserProjections.ProjectToDto())
                .SingleOrDefaultAsync(cancellationToken);

            return user is null
                ? null
                : dataShapingService.ShapeData(user, query.Parameters.Fields);
        }
    }

    internal sealed class UserQueryParameters : BaseQueryParameters;

    internal sealed class UserQueryParametersValidator : AbstractValidator<UserQueryParameters>
    {
        public UserQueryParametersValidator(DataShapingService dataShapingService)
        {
            RuleFor(x => x.Fields)
                .Custom((fields, context) =>
                {
                    if (!dataShapingService.Validate<UserDto>(fields))
                    {
                        context.AddFailure(nameof(context.InstanceToValidate.Fields),
                            $"The provided data shaping fields aren't valid: '{fields}'");
                    }
                });
        }
    }

    internal sealed class UserDto
    {
        public required Guid Id { get; init; }
        public required string UserName { get; init; }
        public required string Email { get; init; }
        public required string FullName { get; init; }
    }

    private static class UserProjections
    {
        public static Expression<Func<User, UserDto>> ProjectToDto()
        {
            return u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FullName = u.FullName
            };
        }
    }
}
