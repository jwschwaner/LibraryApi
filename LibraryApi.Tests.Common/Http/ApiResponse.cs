using System.Net;

namespace LibraryApi.Tests.Common.Http;

/// <param name="Content">
/// The body as <typeparamref name="T"/>; default for an error response or an empty body.
/// </param>
/// <param name="Error">The API's error body (<c>{ "error": { type, message } }</c>) when the call failed.</param>
public sealed record ApiResponse<T>(
    HttpStatusCode StatusCode,
    bool IsSuccessStatusCode,
    string RawBody,
    T? Content,
    ApiError? Error);

public sealed record ApiError(string Type, string Message);
