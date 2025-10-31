using ProjectManagement.Api.Common.Domain.Abstractions;

namespace ProjectManagement.Api.Common.Extensions;

public static class ResultExtensions
{
    public static IResult ToProblemDetails<TValue>(this Result<TValue> result)
    {
        return ((Result)result).ToProblemDetails();
    }

    public static IResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot convert successful result to problem details");
        }

        return result.Error.Code switch
        {
            nameof(Error.NullValue) => Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request"),

            nameof(Error.NotFound) => Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found"),

            nameof(Error.Conflict) => Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict"),

            _ => Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request")
        };
    }
}
