namespace LibraryApi.Application.Results;

/// <summary>
/// Builds a failed CommandResult&lt;T&gt; or QueryResult&lt;T&gt; when only the closed response type is
/// known, as in pipeline behaviors.
/// </summary>
internal static class FailureResult
{
    public static TResponse Create<TResponse>(ErrorType errorType, string errorMessage)
        where TResponse : BaseResult
    {
        // Get the inner result type from TResponse (e.g., TResult from CommandResult<TResult>)
        var resultType = typeof(TResponse).GetGenericArguments()[0];

        // Determine if this is a CommandResult or QueryResult and create appropriate failure
        var factoryType = typeof(TResponse).GetGenericTypeDefinition() == typeof(CommandResult<>)
            ? typeof(CommandResult)
            : typeof(QueryResult);

        var failureMethod = factoryType
            .GetMethod(nameof(CommandResult.Failure), [typeof(ErrorType), typeof(string)])!
            .MakeGenericMethod(resultType);

        return (TResponse)failureMethod.Invoke(null, [errorType, errorMessage])!;
    }
}
