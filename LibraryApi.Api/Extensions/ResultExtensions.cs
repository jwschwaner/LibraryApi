using LibraryApi.Application.Results;
using Microsoft.AspNetCore.Mvc;
using EmptyResult = LibraryApi.Application.Results.EmptyResult;

namespace LibraryApi.Api.Extensions;

/// <summary>Maps application results to HTTP responses, so controllers never branch on outcomes.</summary>
public static class ResultExtensions
{
    public static IActionResult ToActionResult<TResult>(this BaseResult<TResult> result)
    {
        return result.IsSuccess ? Success(result.Result) : Error(result.ErrorType, result.ErrorMessage);
    }

    public static IActionResult ToCreatedResult<TResult>(this BaseResult<TResult> result, Func<TResult, string> location)
    {
        if (!result.IsSuccess)
            return Error(result.ErrorType, result.ErrorMessage);

        return new CreatedResult(location(result.Result!), result.Result);
    }

    private static IActionResult Success<T>(T? value)
    {
        if (value is null || value is EmptyResult)
            return new NoContentResult();

        return new OkObjectResult(value);
    }

    private static IActionResult Error(ErrorType type, string? message)
    {
        var (status, defaultMsg) = Map(type);
        var body = new
        {
            error = new
            {
                type = type.ToString(),
                message = message ?? defaultMsg
            }
        };
        return new ObjectResult(body) { StatusCode = status };
    }

    private static (int Status, string DefaultMessage) Map(ErrorType t) => t switch
    {
        ErrorType.None => (StatusCodes.Status200OK, "Success"),
        ErrorType.BadRequest => (StatusCodes.Status400BadRequest, "Bad request"),
        ErrorType.NotFound => (StatusCodes.Status404NotFound, "Resource not found"),
        ErrorType.Validation => (StatusCodes.Status422UnprocessableEntity, "Validation failed"),
        ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
        _ => (StatusCodes.Status500InternalServerError, "Unhandled error")
    };
}
