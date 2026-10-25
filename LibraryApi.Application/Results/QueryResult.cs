namespace LibraryApi.Application.Results;

public static class QueryResult
{
    public static QueryResult<TResult> Success<TResult>(TResult result) => new(result);

    public static QueryResult<TResult> Failure<TResult>(ErrorType errorType) => new(default, errorType);

    public static QueryResult<TResult> Failure<TResult>(ErrorType errorType, string errorMessage) =>
        new(default, errorType, errorMessage);

    public static QueryResult<TResult> NotFound<TResult>(string? resourceName = null) =>
        new(default, ErrorType.NotFound, resourceName != null ? $"{resourceName} not found" : "Resource not found");

    public static QueryResult<TResult> FromNullable<TResult>(TResult? result, string? resourceName = null)
        where TResult : class
    {
        return result != null ? Success(result) : NotFound<TResult>(resourceName);
    }
}

public class QueryResult<TResult> : BaseResult<TResult>
{
    internal QueryResult(TResult? result, ErrorType errorType = ErrorType.None, string? errorMessage = null)
        : base(result, errorType, errorMessage)
    {
    }
}
