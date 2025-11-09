using System.Dynamic;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Common.Authorization;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.DTOs.Project;
using ProjectManagement.Api.Common.Extensions;
using ProjectManagement.Api.Common.Mappings;
using ProjectManagement.Api.Common.Models;
using ProjectManagement.Api.Common.Persistence;
using ProjectManagement.Api.Common.Services.Auth;
using ProjectManagement.Api.Common.Services.DataShaping;
using ProjectManagement.Api.Common.Services.Sorting;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Projects;

internal sealed class GetProjects : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet(
                "api/projects",
                async (
                    [AsParameters] ProjectsQueryParameters query,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var getProjectsQuery = new GetProjectsQuery(query);
                    var result =
                        await mediator.SendQueryAsync<GetProjectsQuery, Result<PaginationResult<ExpandoObject>>>(
                            getProjectsQuery,
                            cancellationToken);

                    return result.IsSuccess
                        ? Results.Ok(result.Value)
                        : result.ToProblemDetails();
                }
            )
            .WithTags(nameof(Projects))
            .RequirePermissions(Permissions.Projects.Read);
    }

    internal sealed record GetProjectsQuery(ProjectsQueryParameters Parameters)
        : IQuery<Result<PaginationResult<ExpandoObject>>>;

    internal sealed class GetProjectsQueryValidator : AbstractValidator<GetProjectsQuery>
    {
        public GetProjectsQueryValidator(ISortMappingProvider sortMappingProvider,
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
                    if (!sortMappingProvider.ValidateMappings<ProjectDto, Project>(sort))
                    {
                        context.AddFailure(nameof(context.InstanceToValidate.Parameters.Sort),
                            $"The provided sort parameter isn't valid: '{sort}'");
                    }
                });

            RuleFor(x => x.Parameters.Fields)
                .Custom((fields, context) =>
                {
                    if (!dataShapingService.Validate<ProjectDto>(fields))
                    {
                        context.AddFailure(nameof(context.InstanceToValidate.Parameters.Fields),
                            $"The provided data shaping fields aren't valid: '{fields}'");
                    }
                });
        }
    }

    internal sealed class GetProjectsQueryHandler(
        ProjectManagementDbContext dbContext,
        ICurrentUserService currentUserService,
        ISortMappingProvider sortMappingProvider,
        IDataShapingService dataShapingService
    )
        : IQueryHandler<GetProjectsQuery, Result<PaginationResult<ExpandoObject>>>
    {
        public async Task<Result<PaginationResult<ExpandoObject>>> HandleAsync(GetProjectsQuery query,
            CancellationToken cancellationToken = default)
        {
            var userId = currentUserService.UserId;

            if (userId is null)
            {
                return Result.Failure<PaginationResult<ExpandoObject>>(Error.Unauthorized);
            }

            var search = query.Parameters.Search?.Trim();
            var page = query.Parameters.Page ?? ExtendedQueryParameters.DefaultPage;
            var pageSize = query.Parameters.PageSize ?? ExtendedQueryParameters.DefaultPageSize;

            var sortMappings = sortMappingProvider.GetMappings<ProjectDto, Project>();

            var projectsQuery = dbContext
                .Projects
                .Where(p => search == null ||
                            EF.Functions.ILike(p.Name, $"%{search}%") ||
                            EF.Functions.ILike(p.Description, $"%{search}%"))
                .Where(p => p.OwnerId == userId || p.Members.Any(m => m.UserId == userId))
                .ApplySort(query.Parameters.Sort, sortMappings)
                .Select(ProjectMappings.ProjectToProjectDto<ProjectDto>());

            var totalCount = await projectsQuery.CountAsync(cancellationToken);
            var projects = await projectsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var paginationResult = new PaginationResult<ExpandoObject>
            {
                Items = dataShapingService.ShapeCollectionData(
                    projects,
                    query.Parameters.Fields),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return paginationResult;
        }
    }

    internal sealed class ProjectsQueryParameters : ExtendedQueryParameters;
}
