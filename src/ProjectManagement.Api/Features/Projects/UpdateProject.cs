using ProjectManagement.Api.Common.Authorization;
using ProjectManagement.Api.Common.Extensions;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Constants;

namespace ProjectManagement.Api.Features.Projects;

internal sealed class UpdateProject : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut(
                EndpointNames.Projects.Routes.ById, () => Task.FromResult("Not implemented yet"))
            .WithName(EndpointNames.Projects.Names.UpdateProject)
            .WithTags(EndpointNames.Projects.GroupName)
            .RequirePermissions(Permissions.Projects.Write);
    }
}
