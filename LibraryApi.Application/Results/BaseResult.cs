namespace LibraryApi.Application.Results;

public abstract class BaseResult(ErrorType errorType = ErrorType.None, string? errorMessage = null)
{
    public bool IsSuccess { get; } = errorType == ErrorType.None;
    public string? ErrorMessage { get; } = errorMessage;
    public ErrorType ErrorType { get; } = errorType;
}

public abstract class BaseResult<TResult>(
    TResult? result,
    ErrorType errorType = ErrorType.None,
    string? errorMessage = null)
    : BaseResult(errorType, errorMessage)
{
    public TResult? Result { get; } = result;
}
