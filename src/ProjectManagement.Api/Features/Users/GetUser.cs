using System.Dynamic;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Common.DTOs.User;
using ProjectManagement.Api.Common.Extensions;
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
                var result = await mediator.SendQueryAsync<GetUserQuery, Result<ExpandoObject?>>(
                    new GetUserQuery(userId, query),
                    cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.ToProblemDetails();
            }
        );
    }

    internal sealed record GetUserQuery(Guid Id, UserQueryParameters Parameters) : IQuery<Result<ExpandoObject?>>;

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
        : IQueryHandler<GetUserQuery, Result<ExpandoObject?>>
    {
        public async Task<Result<ExpandoObject?>> HandleAsync(GetUserQuery query,
            CancellationToken cancellationToken = default)
        {
            var user = await dbContext
                .Users
                .Where(u => u.Id == query.Id)
                .Select(UserMappings.ProjectToUserDto<UserDto>())
                .SingleOrDefaultAsync(cancellationToken);

            return user is null
                ? Result.Failure<ExpandoObject?>(Error.NotFound)
                : dataShapingService.ShapeData(user, query.Parameters.Fields);
        }
    }

    internal sealed class UserQueryParameters : BaseQueryParameters;
}
