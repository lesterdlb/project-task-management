using ProjectManagement.Api.Common.Authorization;
using ProjectManagement.Api.Common.Extensions;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Constants;

namespace ProjectManagement.Api.Features.Projects;

internal sealed class GetProject : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet(
                EndpointNames.Projects.Routes.ById, () => Task.FromResult("Not implemented yet"))
            .WithName(EndpointNames.Projects.Names.GetProject)
            .WithTags(EndpointNames.Projects.GroupName)
            .RequirePermissions(Permissions.Projects.Read);
    }
}
