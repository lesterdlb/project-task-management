using System.Dynamic;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Common.Authorization;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.DTOs.User;
using ProjectManagement.Api.Common.Extensions;
using ProjectManagement.Api.Common.Mappings;
using ProjectManagement.Api.Common.Models;
using ProjectManagement.Api.Common.Services.DataShaping;
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
                    var result = await mediator.SendQueryAsync<GetUsersQuery, Result<PaginationResult<ExpandoObject>>>(
                        getUsersQuery,
                        cancellationToken);

                    return result.IsSuccess
                        ? Results.Ok(result.Value)
                        : result.ToProblemDetails();
                }
            )
            .WithTags(nameof(Users))
            .RequirePermissions(Permissions.Users.Read);
    }

    internal sealed record GetUsersQuery(UsersQueryParameters Parameters)
        : IQuery<Result<PaginationResult<ExpandoObject>>>;

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
        UserManager<User> userManager,
        ISortMappingProvider sortMappingProvider,
        IDataShapingService dataShapingService
    )
        : IQueryHandler<GetUsersQuery, Result<PaginationResult<ExpandoObject>>>
    {
        public async Task<Result<PaginationResult<ExpandoObject>>> HandleAsync(GetUsersQuery query,
            CancellationToken cancellationToken = default)
        {
            var search = query.Parameters.Search?.Trim();
            var page = query.Parameters.Page ?? ExtendedQueryParameters.DefaultPage;
            var pageSize = query.Parameters.PageSize ?? ExtendedQueryParameters.DefaultPageSize;

            var sortMappings = sortMappingProvider.GetMappings<UserDto, User>();

            var usersQuery = userManager
                .Users
                .Where(u => search == null ||
                            EF.Functions.ILike(u.UserName!, $"%{search}%") ||
                            EF.Functions.ILike(u.FullName, $"%{search}%") ||
                            EF.Functions.ILike(u.Email!, $"%{search}%"))
                .ApplySort(query.Parameters.Sort, sortMappings)
                .Select(UserMappings.ProjectToUserDto<UserDto>());

            var totalCount = await usersQuery.CountAsync(cancellationToken);
            var users = await usersQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var paginationResult = new PaginationResult<ExpandoObject>
            {
                Items = dataShapingService.ShapeCollectionData(
                    users,
                    query.Parameters.Fields),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return paginationResult;
        }
    }

    internal sealed class UsersQueryParameters : ExtendedQueryParameters;
}
