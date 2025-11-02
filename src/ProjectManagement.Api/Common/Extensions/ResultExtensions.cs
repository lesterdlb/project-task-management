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

        return result.Error.Category switch
        {
            ErrorCategory.BadRequest => Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request"),

            ErrorCategory.Unauthorized => Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized"),

            ErrorCategory.Forbidden => Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden"),

            ErrorCategory.NotFound => Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found"),

            ErrorCategory.Conflict => Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict"),

            _ => Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal Server Error")
        };
    }
}
