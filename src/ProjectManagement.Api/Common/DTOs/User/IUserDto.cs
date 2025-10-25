namespace ProjectManagement.Api.Common.DTOs.User;

public interface IUserDto
{
    Guid Id { get; init; }
    string UserName { get; init; }
    string Email { get; init; }
    string FullName { get; init; }
}
