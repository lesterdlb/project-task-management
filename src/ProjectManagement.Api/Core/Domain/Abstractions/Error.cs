namespace ProjectManagement.Api.Core.Domain.Abstractions;

public record Error(string Code, string Message, ErrorCategory Category = ErrorCategory.BadRequest)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static readonly Error NullValue = new(
        nameof(NullValue), "The specified result value is null.");

    public static readonly Error NotFound = new(
        nameof(NotFound), "The requested resource was not found.", ErrorCategory.NotFound);

    public static readonly Error Conflict = new(
        nameof(Conflict), "The resource already exists.", ErrorCategory.Conflict);

    public static readonly Error Unauthorized = new(
        nameof(Unauthorized), "Authentication failed.", ErrorCategory.Unauthorized);

    public static readonly Error Forbidden = new(
        nameof(Forbidden), "Access denied.", ErrorCategory.Forbidden);

    public static class Auth
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

    public static class User
    {
        public static Error CreateFailed(string details) =>
            new("User.CreateFailed", $"User creation failed: {details}");
    }

    public static class Project
    {
        public static Error CreationForbidden =>
            new("Project.CreationForbidden", "Only administrators can create projects for other users");

        public static Error OwnerNotFound =>
            new("Project.OwnerNotFound", "The specified project owner does not exist", ErrorCategory.NotFound);
    }

    public static class ProjectMember
    {
        public static Error UserNotFound =>
            new("ProjectMember.UserNotFound", "The specified user does not exist", ErrorCategory.NotFound);

        public static Error AlreadyMember =>
            new("ProjectMember.AlreadyMember", "User is already a member of this project");

        public static Error OwnerAsMember =>
            new("ProjectMember.OwnerAsMember", "Project owner cannot be added as a member");
    }
}
