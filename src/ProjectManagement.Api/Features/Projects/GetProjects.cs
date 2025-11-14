using System.Dynamic;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Common.Authorization;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.Domain.Enums;
using ProjectManagement.Api.Common.DTOs.Project;
using ProjectManagement.Api.Common.Extensions;
using ProjectManagement.Api.Common.Mappings;
using ProjectManagement.Api.Common.Models;
using ProjectManagement.Api.Common.Persistence;
using ProjectManagement.Api.Common.Services.Auth;
using ProjectManagement.Api.Common.Services.DataShaping;
using ProjectManagement.Api.Common.Services.Links;
using ProjectManagement.Api.Common.Services.Sorting;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Common.Validators;
using ProjectManagement.Api.Constants;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Projects;

internal sealed class GetProjects : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet(
                EndpointNames.Projects.Routes.Base,
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
            .WithName(EndpointNames.Projects.Names.GetProjects)
            .WithTags(EndpointNames.Projects.GroupName)
            .RequirePermissions(Permissions.Projects.Read);
    }

    internal sealed record GetProjectsQuery(ProjectsQueryParameters Parameters)
        : IQuery<Result<PaginationResult<ExpandoObject>>>;

    internal sealed class GetProjectsQueryValidator : AbstractValidator<GetProjectsQuery>
    {
        public GetProjectsQueryValidator(ISortMappingProvider sortMappingProvider,
            IDataShapingService dataShapingService)
        {
            RuleFor(x => x.Parameters.Page).ValidatePage();
            RuleFor(x => x.Parameters.PageSize).ValidatePageSize();
            RuleFor(x => x.Parameters.Sort).ValidateSort<GetProjectsQuery, ProjectDto, Project>(sortMappingProvider);
            RuleFor(x => x.Parameters.Fields).ValidateFields<GetProjectsQuery, ProjectDto>(dataShapingService);
        }
    }

    internal sealed class GetProjectsQueryHandler(
        ProjectManagementDbContext dbContext,
        ICurrentUserService currentUserService,
        ISortMappingProvider sortMappingProvider,
        IDataShapingService dataShapingService,
        ILinkService linkService
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

            var isAdmin = currentUserService.IsInRole(nameof(UserRole.Admin));

            var search = query.Parameters.Search?.Trim();
            var page = query.Parameters.Page ?? ExtendedQueryParameters.DefaultPage;
            var pageSize = query.Parameters.PageSize ?? ExtendedQueryParameters.DefaultPageSize;

            var sortMappings = sortMappingProvider.GetMappings<ProjectDto, Project>();

            var projectsQuery = dbContext
                .Projects
                .Where(p => search == null ||
                            EF.Functions.ILike(p.Name, $"%{search}%") ||
                            EF.Functions.ILike(p.Description, $"%{search}%"))
                .Where(p => isAdmin ||
                            p.OwnerId == userId ||
                            p.Members.Any(m => m.UserId == userId))
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
                    query.Parameters.Fields,
                    query.Parameters.IncludeLinks
                        ? u => linkService.CreateLinksForItem(
                            EndpointNames.Projects.Names.GetProject,
                            EndpointNames.Projects.Names.UpdateProject,
                            EndpointNames.Projects.Names.DeleteProject,
                            u.Id,
                            query.Parameters.Fields)
                        : null),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            if (query.Parameters.IncludeLinks)
            {
                paginationResult.AddLinks(linkService.CreateLinksForCollection(
                    EndpointNames.Projects.Names.GetProjects,
                    EndpointNames.Projects.Names.CreateProject,
                    query.Parameters,
                    paginationResult.HasNextPage,
                    paginationResult.HasPreviousPage));
            }

            return paginationResult;
        }
    }

    internal sealed class ProjectsQueryParameters : ExtendedQueryParameters;
}
