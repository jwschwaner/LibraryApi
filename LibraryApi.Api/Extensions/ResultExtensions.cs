using LibraryApi.Application.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using EmptyResult = LibraryApi.Application.Results.EmptyResult;

namespace LibraryApi.Api.Extensions;

/// <summary>
/// Maps application results to HTTP responses, so controllers never branch on outcomes. Errors
/// are ProblemDetails (RFC 9457), built by ASP.NET Core's own factory, so they have the same
/// fields (type, title, status, traceId) as the errors the framework produces itself, such as a
/// request body that can't be read.
/// </summary>
public static class ResultExtensions
{
    public static IActionResult ToActionResult<TResult>(this BaseResult<TResult> result)
    {
        return result.IsSuccess ? Success(result.Result) : new ProblemResult(result);
    }

    public static IActionResult ToCreatedResult<TResult>(this BaseResult<TResult> result, Func<TResult, string> location)
    {
        if (!result.IsSuccess)
            return new ProblemResult(result);

        return new CreatedResult(location(result.Result!), result.Result);
    }

    private static IActionResult Success<T>(T? value)
    {
        if (value is null || value is EmptyResult)
            return new NoContentResult();

        return new OkObjectResult(value);
    }

    private static int StatusCode(ErrorType t) => t switch
    {
        ErrorType.BadRequest => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Validation => StatusCodes.Status422UnprocessableEntity,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    /// <summary>
    /// Builds the ProblemDetails when the response is written, because the factory needs the
    /// request (for the trace id).
    /// </summary>
    private sealed class ProblemResult(BaseResult result) : IActionResult
    {
        public Task ExecuteResultAsync(ActionContext context)
        {
            var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            var status = StatusCode(result.ErrorType);

            ProblemDetails problem;
            if (result.ValidationErrors is { Count: > 0 } errors)
            {
                // Validation failures list their messages per field
                var modelState = new ModelStateDictionary();
                foreach (var (field, messages) in errors)
                foreach (var message in messages)
                    modelState.AddModelError(field, message);

                problem = factory.CreateValidationProblemDetails(context.HttpContext, modelState, status);
            }
            else
            {
                problem = factory.CreateProblemDetails(context.HttpContext, status, detail: result.ErrorMessage);
            }

            // The broken business rule's stable code, for clients that react to a specific rule
            if (result.ErrorCode is not null)
                problem.Extensions["code"] = result.ErrorCode;

            return new ObjectResult(problem) { StatusCode = status }.ExecuteResultAsync(context);
        }
    }
}
