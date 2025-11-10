namespace ProjectManagement.Api.Common.Models;

public sealed class LinkDto
{
    public required string Href { get; init; }
    public required string Rel { get; init; }
    public required string Method { get; init; }
    public string? Type { get; init; }
    public string? Title { get; init; }
    public bool? Deprecated { get; init; }
}
