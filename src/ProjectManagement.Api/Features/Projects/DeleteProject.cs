using ProjectManagement.Api.Common.Authorization;
using ProjectManagement.Api.Common.Extensions;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Constants;

namespace ProjectManagement.Api.Features.Projects;

internal sealed class DeleteProject : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete(
                EndpointNames.Projects.Routes.ById, () => Task.FromResult("Not implemented yet"))
            .WithName(EndpointNames.Projects.Names.DeleteProject)
            .WithTags(EndpointNames.Projects.GroupName)
            .RequirePermissions(Permissions.Projects.Delete);
    }
}
