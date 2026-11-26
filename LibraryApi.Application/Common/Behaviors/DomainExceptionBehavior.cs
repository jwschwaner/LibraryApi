using LibraryApi.Application.Results;
using LibraryApi.Domain.Common;
using Mediator;
using Microsoft.Extensions.Logging;

namespace LibraryApi.Application.Common.Behaviors;

/// <summary>
/// Turns a <see cref="DomainException"/> thrown by a handler into a Conflict result that carries
/// the rule's code. Handlers call the domain without a try/catch of their own, and every caller
/// (a controller, a background job) gets a result back instead of an exception.
/// </summary>
/// <remarks>
/// Anything the handler wrote before the exception stays written, so handlers call the domain
/// before they write.
/// </remarks>
public sealed class DomainExceptionBehavior<TRequest, TResponse>(
    ILogger<DomainExceptionBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
    where TResponse : BaseResult
{
    public async ValueTask<TResponse> Handle(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(message, cancellationToken);
        }
        catch (DomainException ex)
        {
            logger.LogInformation("{RequestType} refused by rule {Code}: {Message}",
                typeof(TRequest).Name, ex.Code, ex.Message);

            return FailureResult.Create<TResponse>(ErrorType.Conflict, ex.Message, ex.Code);
        }
    }
}
