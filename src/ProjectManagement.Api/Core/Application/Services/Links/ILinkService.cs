using ProjectManagement.Api.Core.Application.Models;

namespace ProjectManagement.Api.Core.Application.Services.Links;

public interface ILinkService
{
    List<LinkDto> CreateLinksForItem(
        string getEndpointName,
        string updateEndpointName,
        string deleteEndpointName,
        Guid id,
        string? fields = null);

    List<LinkDto> CreateLinksForCollection<TParams>(
        string getEndpointName,
        string createEndpointName,
        TParams parameters,
        bool hasNextPage,
        bool hasPreviousPage) where TParams : ExtendedQueryParameters;

    string CreateHref(string endpointName, object? values);
}
