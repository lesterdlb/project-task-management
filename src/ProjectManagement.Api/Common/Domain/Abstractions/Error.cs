namespace ProjectManagement.Api.Common.Domain.Abstractions;

public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new(nameof(NullValue), "The specified result value is null.");
    public static readonly Error NotFound = new(nameof(NotFound), "The requested resource was not found.");
    public static readonly Error Conflict = new(nameof(Conflict), "The resource already exists.");
}
