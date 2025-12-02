namespace ProjectManagement.Api.Infrastructure.Slices;

public static class EndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder endpointRouteBuilder)
    {
        public void MapSliceEndpoints()
        {
            foreach (var slice in endpointRouteBuilder.ServiceProvider.GetServices<ISlice>())
            {
                slice.AddEndpoint(endpointRouteBuilder);
            }
        }
    }
}
