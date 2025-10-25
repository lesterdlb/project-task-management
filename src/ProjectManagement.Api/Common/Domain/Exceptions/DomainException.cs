namespace ProjectManagement.Api.Common.Domain.Exceptions;

public sealed class DomainException(string message) : Exception(message);
