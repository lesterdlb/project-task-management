namespace ProjectManagement.Api.Common.Models;

public abstract class ExtendedQueryParameters : BaseQueryParameters
{
    public string? Search { get; set; }
    public string? Sort { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
