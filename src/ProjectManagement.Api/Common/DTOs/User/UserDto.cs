namespace ProjectManagement.Api.Common.DTOs.User;

public class UserDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; }
    public string Email { get; init; }
    public string FullName { get; init; }
}
