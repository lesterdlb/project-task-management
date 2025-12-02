namespace ProjectManagement.Api.Core.Application.Models;

public abstract class ExtendedQueryParameters : BaseQueryParameters
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;

    public string? Search { get; set; }
    public string? Sort { get; init; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
