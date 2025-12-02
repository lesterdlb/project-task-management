using ProjectManagement.Api.Core.Domain.Abstractions;

namespace ProjectManagement.Api.Features.Users;

public static class UserErrors
{
    public static Error CreateFailed(string details) =>
        new("User.CreateFailed", $"User creation failed: {details}");
}
