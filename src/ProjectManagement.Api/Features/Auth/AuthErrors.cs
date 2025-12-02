using ProjectManagement.Api.Core.Domain.Abstractions;

namespace ProjectManagement.Api.Features.Auth;

public static class AuthErrors
{
    public static Error InvalidCredentials =>
        new("Auth.InvalidCredentials", "Invalid email or password", ErrorCategory.Unauthorized);

    public static Error EmailNotConfirmed =>
        new("Auth.EmailNotConfirmed", "Please confirm your email address", ErrorCategory.Unauthorized);

    public static Error EmailAlreadyConfirmed =>
        new("Auth.EmailAlreadyConfirmed", "Email already confirmed", ErrorCategory.Conflict);

    public static Error UserNotFound =>
        new("Auth.UserNotFound", "User not found", ErrorCategory.NotFound);

    public static Error RegistrationFailed(string details) =>
        new("Auth.RegistrationFailed", $"Registration failed: {details}");

    public static Error EmailConfirmationFailed(string details) =>
        new("Auth.EmailConfirmationFailed", $"Email confirmation failed: {details}");
}
