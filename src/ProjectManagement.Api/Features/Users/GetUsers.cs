using System.Dynamic;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.DTOs.User;
using ProjectManagement.Api.Common.Mappings;
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
        );
    }

    internal sealed record GetUsersQuery(UsersQueryParameters Parameters) : IQuery<PaginationResult<ExpandoObject>>;

    internal sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
    {
        public GetUsersQueryValidator(ISortMappingProvider sortMappingProvider,
            IDataShapingService dataShapingService)
        {
            RuleFor(x => x.Parameters.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0.");

            RuleFor(x => x.Parameters.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("PageSize must be between 1 and 100.");

            RuleFor(x => x.Parameters.Sort)
                .Custom((sort, context) =>
                {
                    if (!sortMappingProvider.ValidateMappings<UserDto, User>(sort))
                    {
                        context.AddFailure(nameof(context.InstanceToValidate.Parameters.Sort),
                            $"The provided sort parameter isn't valid: '{sort}'");
                    }
                });

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

    internal sealed class GetUsersQueryHandler(
        ProjectManagementDbContext dbContext,
        ISortMappingProvider sortMappingProvider,
        IDataShapingService dataShapingService
    )
        : IQueryHandler<GetUsersQuery, PaginationResult<ExpandoObject>>
    {
        public async Task<PaginationResult<ExpandoObject>> HandleAsync(GetUsersQuery query,
            CancellationToken cancellationToken = default)
        {
            query.Parameters.Search ??= query.Parameters.Search?.Trim().ToLower();

            var sortMappings = sortMappingProvider.GetMappings<IUserDto, User>();

            var usersQuery = dbContext
                .Users
                .Where(u => query.Parameters.Search == null ||
                            u.UserName.ToLower().Contains(query.Parameters.Search) ||
                            u.FullName.ToLower().Contains(query.Parameters.Search))
                .ApplySort(query.Parameters.Sort, sortMappings)
                .Select(UserMappings.ProjectToUserDto<UserDto>());

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

    internal sealed class UsersQueryParameters : ExtendedQueryParameters;

    private sealed class UserDto : IUserDto
    {
        public Guid Id { get; init; }
        public string UserName { get; init; }
        public string Email { get; init; }
        public string FullName { get; init; }
    }
}
