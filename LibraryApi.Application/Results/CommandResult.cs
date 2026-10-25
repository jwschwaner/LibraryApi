namespace LibraryApi.Application.Results;

public static class CommandResult
{
    public static CommandResult<TResult> Success<TResult>(TResult result) => new(result);

    public static CommandResult<EmptyResult> Success() => new(EmptyResult.Value);

    public static CommandResult<TResult> Created<TResult>(TResult createdEntity) => new(createdEntity);

    public static CommandResult<TResult> Failure<TResult>(ErrorType errorType) => new(default, errorType);

    public static CommandResult<TResult> Failure<TResult>(ErrorType errorType, string errorMessage) =>
        new(default, errorType, errorMessage);

    public static CommandResult<TResult> NotFound<TResult>(string? resourceName = null) =>
        new(default, ErrorType.NotFound, resourceName != null ? $"{resourceName} not found" : "Resource not found");

    public static CommandResult<TResult> Conflict<TResult>(string conflictMessage) =>
        new(default, ErrorType.Conflict, conflictMessage);
}

public class CommandResult<TResult> : BaseResult<TResult>
{
    internal CommandResult(TResult? result, ErrorType errorType = ErrorType.None, string? errorMessage = null)
        : base(result, errorType, errorMessage)
    {
    }
}
