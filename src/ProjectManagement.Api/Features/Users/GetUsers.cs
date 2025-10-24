using System.Dynamic;
using System.Linq.Expressions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.Filters;
using ProjectManagement.Api.Common.Models;
using ProjectManagement.Api.Common.Persistence;
using ProjectManagement.Api.Common.Services;
using ProjectManagement.Api.Common.Services.Sorting;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Users;

internal sealed class GetUsers : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet(
                "api/users",
                async (
                    [AsParameters] UsersQueryParameters query,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var getUsersQuery = new GetUsersQuery(query);
                    return Results.Ok(await mediator.SendQueryAsync<GetUsersQuery, PaginationResult<ExpandoObject>>(
                        getUsersQuery,
                        cancellationToken));
                }
            )
            .AddEndpointFilter<ValidationFilter<UsersQueryParameters>>();
    }

    internal sealed record GetUsersQuery(UsersQueryParameters Parameters) : IQuery;

    internal sealed class GetUsersQueryHandler(
        ProjectManagementDbContext dbContext,
        SortMappingProvider sortMappingProvider,
        DataShapingService dataShapingService
    )
        : IQueryHandler<GetUsersQuery, PaginationResult<ExpandoObject>>
    {
        public async Task<PaginationResult<ExpandoObject>> HandleAsync(GetUsersQuery query,
            CancellationToken cancellationToken = default)
        {
            query.Parameters.Search ??= query.Parameters.Search?.Trim().ToLower();

            var sortMappings = sortMappingProvider.GetMappings<UserDto, User>();

            var usersQuery = dbContext
                .Users
                .Where(u => query.Parameters.Search == null ||
                            u.UserName.ToLower().Contains(query.Parameters.Search) ||
                            u.FullName.ToLower().Contains(query.Parameters.Search))
                .ApplySort(query.Parameters.Sort, sortMappings)
                .Select(UserQueries.ProjectToDto());

            var totalCount = await usersQuery.CountAsync(cancellationToken);
            var users = await usersQuery
                .Skip((query.Parameters.Page - 1) * query.Parameters.PageSize)
                .Take(query.Parameters.PageSize)
                .ToListAsync(cancellationToken);

            var paginationResult = new PaginationResult<ExpandoObject>
            {
                Items = dataShapingService.ShapeCollectionData(
                    users,
                    query.Parameters.Fields),
                Page = query.Parameters.Page,
                PageSize = query.Parameters.PageSize,
                TotalCount = totalCount
            };

            return paginationResult;
        }
    }

    internal sealed class UserDto
    {
        public required string UserName { get; init; }
        public required string Email { get; init; }
        public required string FullName { get; init; }
    }

    private static class UserQueries
    {
        public static Expression<Func<User, UserDto>> ProjectToDto()
        {
            return h => new UserDto
            {
                UserName = h.UserName,
                Email = h.Email,
                FullName = h.FullName
            };
        }
    }

    internal sealed class UsersQueryParameters : BaseQueryParameters;

    internal sealed class UsersQueryParametersValidator : AbstractValidator<UsersQueryParameters>
    {
        public UsersQueryParametersValidator(SortMappingProvider sortMappingProvider,
            DataShapingService dataShapingService)
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("PageSize must be between 1 and 100.");

            RuleFor(x => x.Sort)
                .Custom((sort, context) =>
                {
                    if (!sortMappingProvider.ValidateMappings<UserDto, User>(sort))
                    {
                        context.AddFailure(nameof(context.InstanceToValidate.Sort),
                            $"The provided sort parameter isn't valid: '{sort}'");
                    }
                });

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

    internal sealed class UserMappings
    {
        public static readonly SortMappingDefinition<UserDto, User> SortMapping = new()
        {
            Mappings =
            [
                new SortMapping(nameof(UserDto.UserName), nameof(User.UserName)),
                new SortMapping(nameof(UserDto.Email), nameof(User.Email)),
                new SortMapping(nameof(UserDto.FullName), nameof(User.FullName))
            ]
        };
    }
}
