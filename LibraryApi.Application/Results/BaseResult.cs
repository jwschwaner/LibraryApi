namespace LibraryApi.Application.Results;

public abstract class BaseResult(
    ErrorType errorType = ErrorType.None,
    string? errorMessage = null,
    string? errorCode = null,
    IReadOnlyDictionary<string, string[]>? validationErrors = null)
{
    public bool IsSuccess { get; } = errorType == ErrorType.None;
    public string? ErrorMessage { get; } = errorMessage;
    public ErrorType ErrorType { get; } = errorType;

    /// <summary>The stable code of the business rule that refused the request, when there is one.</summary>
    public string? ErrorCode { get; } = errorCode;

    /// <summary>The messages per field when validation failed.</summary>
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; } = validationErrors;
}

public abstract class BaseResult<TResult>(
    TResult? result,
    ErrorType errorType = ErrorType.None,
    string? errorMessage = null,
    string? errorCode = null,
    IReadOnlyDictionary<string, string[]>? validationErrors = null)
    : BaseResult(errorType, errorMessage, errorCode, validationErrors)
{
    public TResult? Result { get; } = result;
}
