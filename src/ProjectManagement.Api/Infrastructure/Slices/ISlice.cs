namespace ProjectManagement.Api.Infrastructure.Slices;

public interface ISlice
{
    void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder);
}
