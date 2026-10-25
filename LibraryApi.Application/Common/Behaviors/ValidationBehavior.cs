using FluentValidation;
using LibraryApi.Application.Results;
using Mediator;
using Microsoft.Extensions.Logging;

namespace LibraryApi.Application.Common.Behaviors;

/// <summary>
/// Runs every FluentValidation validator for the message before its handler. Handlers can
/// therefore assume their input is well-formed and only check state (exists, available, ...).
/// </summary>
public class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
    where TResponse : BaseResult
{
    public async ValueTask<TResponse> Handle(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(message, cancellationToken);
        }

        var context = new ValidationContext<TRequest>(message);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next(message, cancellationToken);
        }

        var errorMessage = string.Join(", ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}"));

        logger.LogWarning("Validation errors occurred for {RequestType}: {Errors}",
            typeof(TRequest).Name,
            errorMessage);

        return FailureResult.Create<TResponse>(ErrorType.Validation, errorMessage);
    }
}
