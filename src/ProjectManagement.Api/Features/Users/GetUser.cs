using System.Dynamic;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Common.DTOs.User;
using ProjectManagement.Api.Common.Mappings;
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
        );
    }

    internal sealed record GetUserQuery(Guid Id, UserQueryParameters Parameters) : IQuery<ExpandoObject?>;

    internal sealed class GetUserQueryValidator : AbstractValidator<GetUserQuery>
    {
        public GetUserQueryValidator(IDataShapingService dataShapingService)
        {
            RuleFor(x => x.Parameters.Fields)
                .Custom((fields, context) =>
                {
                    if (!dataShapingService.Validate<UserDto>(fields))
                    {
                        context.AddFailure(nameof(context.InstanceToValidate.Parameters.Fields),
                            $"The provided data shaping fields aren't valid: '{fields}'");
                    }
                });
        }
    }

    internal sealed class GetUserQueryHandler(
        ProjectManagementDbContext dbContext,
        IDataShapingService dataShapingService
    )
        : IQueryHandler<GetUserQuery, ExpandoObject?>
    {
        public async Task<ExpandoObject?> HandleAsync(GetUserQuery query, CancellationToken cancellationToken = default)
        {
            var user = await dbContext
                .Users
                .Where(u => u.Id == query.Id)
                .Select(UserMappings.ProjectToUserDto<UserDto>())
                .SingleOrDefaultAsync(cancellationToken);

            return user is null
                ? null
                : dataShapingService.ShapeData(user, query.Parameters.Fields);
        }
    }

    internal sealed class UserQueryParameters : BaseQueryParameters;

    private sealed class UserDto : IUserDto
    {
        public Guid Id { get; init; }
        public string UserName { get; init; }
        public string Email { get; init; }
        public string FullName { get; init; }
    }
}
