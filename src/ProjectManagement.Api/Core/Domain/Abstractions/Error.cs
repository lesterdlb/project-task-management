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
}
