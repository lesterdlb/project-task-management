namespace ProjectManagement.Api.Common.Models;

public sealed record PaginationResult<T> : ICollectionResponse<T>, ILinksResponse
{
    public List<T> Items { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public List<LinkDto> Links { get; init; } = [];

    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
    private int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public void AddLinks(List<LinkDto> links)
    {
        Links.AddRange(links);
    }
}
