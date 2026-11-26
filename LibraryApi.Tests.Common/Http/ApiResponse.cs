using System.Net;

namespace LibraryApi.Tests.Common.Http;

/// <param name="Content">
/// The body as <typeparamref name="T"/>; default for an error response or an empty body.
/// </param>
/// <param name="Error">The API's ProblemDetails error body when the call failed.</param>
public sealed record ApiResponse<T>(
    HttpStatusCode StatusCode,
    bool IsSuccessStatusCode,
    string RawBody,
    T? Content,
    ApiProblem? Error);

/// <summary>The fields of a ProblemDetails body (RFC 9457) that the tests assert on.</summary>
/// <param name="Code">The broken business rule's code, for a 409 from a DomainException.</param>
/// <param name="Errors">The messages per field, for a 422 validation failure.</param>
public sealed record ApiProblem(
    int? Status,
    string? Title,
    string? Detail,
    string? Code,
    Dictionary<string, string[]>? Errors);
