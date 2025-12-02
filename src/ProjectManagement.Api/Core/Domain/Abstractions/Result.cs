using System.Diagnostics.CodeAnalysis;

namespace ProjectManagement.Api.Core.Domain.Abstractions;

public class Result
{
    public Error Error { get; }
    public bool IsSuccess { get; }
    public static Result Success() => new(true, Error.None);
    private static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    protected static Result<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException();
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException();
        }

        IsSuccess = isSuccess;
        Error = error;
    }
}

public class Result<TValue> : Result
{
    protected internal Result(TValue? value, bool isSuccess, Error error) : base(isSuccess, error)
        => Value = value;

    [field: AllowNull, MaybeNull]
    public TValue Value => IsSuccess
        ? field
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    public static implicit operator Result<TValue>(TValue? value) => Create(value);
}
