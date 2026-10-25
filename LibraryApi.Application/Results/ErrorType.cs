namespace LibraryApi.Application.Results;

public enum ErrorType
{
    None = 0,
    BadRequest,
    NotFound,
    Validation,
    Conflict,
    UnhandledError
}
