using System.Dynamic;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Core.Application.Authorization;
using ProjectManagement.Api.Core.Domain.Abstractions;
using ProjectManagement.Api.Core.Domain.Entities;
using ProjectManagement.Api.Core.Application.DTOs.User;
using ProjectManagement.Api.Infrastructure.Extensions;
using ProjectManagement.Api.Core.Application.Mappings;
using ProjectManagement.Api.Core.Application.Models;
using ProjectManagement.Api.Core.Application.Services.DataShaping;
using ProjectManagement.Api.Core.Application.Services.Links;
using ProjectManagement.Api.Infrastructure.Slices;
using ProjectManagement.Api.Constants;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Users;

internal sealed class GetUser : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet(
                EndpointNames.Users.Routes.ById,
                async (
                    Guid id,
                    [AsParameters] UserQueryParameters query,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.SendQueryAsync<GetUserQuery, Result<ExpandoObject?>>(
                        new GetUserQuery(id, query),
                        cancellationToken);

                    return result.IsSuccess
                        ? Results.Ok(result.Value)
                        : result.ToProblemDetails();
                }
            )
            .WithName(EndpointNames.Users.Names.GetUser)
            .WithTags(EndpointNames.Users.GroupName)
            .RequirePermissions(Permissions.Users.Read);
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
        UserManager<User> userManager,
        IDataShapingService dataShapingService,
        ILinkService linkService
    )
        : IQueryHandler<GetUserQuery, Result<ExpandoObject?>>
    {
        public async Task<Result<ExpandoObject?>> HandleAsync(GetUserQuery query,
            CancellationToken cancellationToken = default)
        {
            var user = await userManager.Users
                .Where(u => u.Id == query.Id)
                .Select(UserMappings.ProjectToUserDto<UserDto>())
                .SingleOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return Result.Failure<ExpandoObject?>(Error.NotFound);
            }

            var shapedUser = dataShapingService.ShapeData(user, query.Parameters.Fields);

            if (query.Parameters.IncludeLinks)
            {
                (shapedUser as IDictionary<string, object?>)["links"] =
                    linkService.CreateLinksForItem(
                        EndpointNames.Users.Names.GetUser,
                        EndpointNames.Users.Names.UpdateUser,
                        EndpointNames.Users.Names.DeleteUser,
                        user.Id,
                        query.Parameters.Fields);
            }

            return shapedUser;
        }
    }

    internal sealed class UserQueryParameters : BaseQueryParameters;
}
