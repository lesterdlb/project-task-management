namespace ProjectManagement.Api.Core.Application.Models;

public interface ILinksResponse
{
    List<LinkDto> Links { get; init; }
}
