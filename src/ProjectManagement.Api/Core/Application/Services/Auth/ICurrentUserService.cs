namespace ProjectManagement.Api.Core.Application.Services.Auth;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
