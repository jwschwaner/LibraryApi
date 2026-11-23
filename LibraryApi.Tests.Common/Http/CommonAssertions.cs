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

    /// <summary>A 409 whose message names the rule that was broken.</summary>
    public static void IsConflict<T>(ApiResponse<T> response, string expectedMessage)
    {
        StatusCodeMatches(response, HttpStatusCode.Conflict);
        Assert.Equal(expectedMessage, response.Error?.Message);
    }
}
