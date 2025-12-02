using ProjectManagement.Api.Core.Application.Models;

namespace ProjectManagement.Api.Core.Application.DTOs.User;

public record UserDto : ILinksResponse
{
    public Guid Id { get; init; }
    public string UserName { get; init; }
    public string Email { get; init; }
    public string FullName { get; init; }
    public List<LinkDto> Links { get; init; } = [];
}
