using ProjectManagement.Api.Common.Models;

namespace ProjectManagement.Api.Common.Services.Links;

public sealed class LinkService(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    : ILinkService
{
    private readonly LinkGenerator _linkGenerator =
        linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));

    private readonly IHttpContextAccessor _httpContextAccessor =
        httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    public List<LinkDto> CreateLinksForItem(
        string getEndpointName,
        string updateEndpointName,
        string deleteEndpointName,
        Guid id,
        string? fields = null)
    {
        List<LinkDto> links =
        [
            Create(getEndpointName, "self", HttpMethods.Get, new { id, fields }),
            Create(updateEndpointName, "update", HttpMethods.Put, new { id }, "Update this resource",
                "application/json"),
            Create(deleteEndpointName, "delete", HttpMethods.Delete, new { id }, "Delete this resource")
        ];
        return links;
    }

    public List<LinkDto> CreateLinksForCollection<TParams>(
        string getEndpointName,
        string createEndpointName,
        TParams parameters,
        bool hasNextPage,
        bool hasPreviousPage) where TParams : ExtendedQueryParameters
    {
        List<LinkDto> links =
        [
            Create(getEndpointName, "self", HttpMethods.Get, new
            {
                page = parameters.Page,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                search = parameters.Search,
                sort = parameters.Sort,
            }),
            Create(createEndpointName, "create", HttpMethods.Post, values: null, "Create a new resource",
                "application/json")
        ];

        if (hasNextPage)
        {
            links.Add(Create(getEndpointName, "next-page", HttpMethods.Get, new
            {
                page = parameters.Page + 1,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                search = parameters.Search,
                sort = parameters.Sort,
            }, "Next page of results"));
        }

        if (hasPreviousPage)
        {
            links.Add(Create(getEndpointName, "previous-page", HttpMethods.Get, new
            {
                page = parameters.Page - 1,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                search = parameters.Search,
                sort = parameters.Sort,
            }, "Previous page of results"));
        }

        return links;
    }

    public string CreateHref(string endpointName, object? values)
    {
        var httpContext = _httpContextAccessor.HttpContext
                          ?? throw new InvalidOperationException("HttpContext is not available");

        return _linkGenerator.GetUriByName(httpContext, endpointName, values);
    }

    private LinkDto Create(
        string endpointName,
        string rel,
        string method,
        object? values = null,
        string? title = null,
        string? type = null,
        bool deprecated = false)
    {
        var href = CreateHref(endpointName, values);

        return new LinkDto
        {
            Href = href ?? throw new InvalidOperationException(
                $"Could not generate URI for endpoint '{endpointName}'. " +
                $"Ensure the endpoint is registered with .WithName(\"{endpointName}\")"),
            Rel = rel,
            Method = method,
            Type = type,
            Title = title,
            Deprecated = deprecated ? true : null
        };
    }
}
