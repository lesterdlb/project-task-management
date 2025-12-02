using System.Dynamic;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Core.Application.Authorization;
using ProjectManagement.Api.Core.Domain.Abstractions;
using ProjectManagement.Api.Core.Domain.Enums;
using ProjectManagement.Api.Core.Application.DTOs.Project;
using ProjectManagement.Api.Infrastructure.Extensions;
using ProjectManagement.Api.Core.Application.Mappings;
using ProjectManagement.Api.Core.Application.Models;
using ProjectManagement.Api.Infrastructure.Persistence;
using ProjectManagement.Api.Core.Application.Services.Auth;
using ProjectManagement.Api.Core.Application.Services.DataShaping;
using ProjectManagement.Api.Core.Application.Services.Links;
using ProjectManagement.Api.Infrastructure.Slices;
using ProjectManagement.Api.Core.Application.Validation.Common;
using ProjectManagement.Api.Core.Application.Validation.Entities;
using ProjectManagement.Api.Constants;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Projects;

internal sealed class GetProject : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet(
                EndpointNames.Projects.Routes.ById,
                async (
                    Guid id,
                    [AsParameters] ProjectQueryParameters query,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.SendQueryAsync<GetProjectQuery, Result<ExpandoObject?>>(
                        new GetProjectQuery(id, query),
                        cancellationToken);

                    return result.IsSuccess
                        ? Results.Ok(result.Value)
                        : result.ToProblemDetails();
                }
            )
            .WithName(EndpointNames.Projects.Names.GetProject)
            .WithTags(EndpointNames.Projects.GroupName)
            .RequirePermissions(Permissions.Projects.Read);
    }

    internal sealed record GetProjectQuery(Guid Id, ProjectQueryParameters Parameters) : IQuery<Result<ExpandoObject?>>;

    internal sealed class GetProjectQueryValidator : AbstractValidator<GetProjectQuery>
    {
        public GetProjectQueryValidator(IDataShapingService dataShapingService)
        {
            RuleFor(x => x.Parameters.Fields).ValidateFields<GetProjectQuery, ProjectDto>(dataShapingService);
        }
    }

    internal sealed class GetProjectQueryHandler(
        ProjectManagementDbContext dbContext,
        ICurrentUserService currentUserService,
        IDataShapingService dataShapingService,
        ILinkService linkService
    )
        : IQueryHandler<GetProjectQuery, Result<ExpandoObject?>>
    {
        public async Task<Result<ExpandoObject?>> HandleAsync(GetProjectQuery query,
            CancellationToken cancellationToken = default)
        {
            var userId = currentUserService.UserId;
            if (userId is null)
            {
                return Result.Failure<ExpandoObject?>(Error.Unauthorized);
            }
            
            var isAdmin = currentUserService.IsInRole(nameof(UserRole.Admin));


            var project = await dbContext.Projects
                .Where(p => p.Id == query.Id && 
                            (isAdmin || 
                             p.OwnerId == userId ||
                             p.Members.Any(m => m.UserId == userId)))
                .Select(ProjectMappings.ProjectToProjectDto<ProjectDto>())
                .SingleOrDefaultAsync(cancellationToken);

            if (project is null)
            {
                return Result.Failure<ExpandoObject?>(Error.NotFound);
            }

            var shapedProject = dataShapingService.ShapeData(project, query.Parameters.Fields);

            if (query.Parameters.IncludeLinks)
            {
                (shapedProject as IDictionary<string, object?>)["links"] =
                    linkService.CreateLinksForItem(
                        EndpointNames.Projects.Names.GetProject,
                        EndpointNames.Projects.Names.UpdateProject,
                        EndpointNames.Projects.Names.DeleteProject,
                        project.Id,
                        query.Parameters.Fields);
            }

            return shapedProject;
        }
    }

    internal sealed class ProjectQueryParameters : BaseQueryParameters;
}
