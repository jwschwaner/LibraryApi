using System.Net;
using Xunit;

namespace LibraryApi.Tests.Common.Http;

public static class CommonAssertions
{
    public static void IsSuccessStatusCode<T>(ApiResponse<T> response)
    {
        Assert.True(
            response.IsSuccessStatusCode,
            $"Expected: success status code{Environment.NewLine}Actual: {response.StatusCode} {response.RawBody}");
    }

    public static void StatusCodeMatches<T>(ApiResponse<T> response, HttpStatusCode expected)
    {
        Assert.True(
            response.StatusCode == expected,
            $"Expected: {expected}{Environment.NewLine}Actual: {response.StatusCode} {response.RawBody}");
    }

    /// <summary>
    /// A 409 ProblemDetails whose detail names the rule that was broken. Pass the code from the
    /// entity's <c>Errors</c> class when the rule is a domain rule.
    /// </summary>
    public static void IsConflict<T>(ApiResponse<T> response, string expectedMessage, string? expectedCode = null)
    {
        StatusCodeMatches(response, HttpStatusCode.Conflict);
        Assert.Equal(409, response.Error?.Status);
        Assert.Equal(expectedMessage, response.Error?.Detail);
        Assert.Equal(expectedCode, response.Error?.Code);
    }

    /// <summary>A 422 ValidationProblemDetails with errors for exactly these fields.</summary>
    public static void IsValidationError<T>(ApiResponse<T> response, params string[] expectedFields)
    {
        StatusCodeMatches(response, HttpStatusCode.UnprocessableEntity);
        Assert.Equal(422, response.Error?.Status);
        Assert.NotNull(response.Error?.Errors);
        Assert.Equal(expectedFields.Order(), response.Error.Errors.Keys.Order());
    }
}
