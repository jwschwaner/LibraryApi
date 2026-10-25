using LibraryApi.Domain.Common;

namespace LibraryApi.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (DomainException de)
        {
            // A rule a handler didn't translate into a result; still the caller's fault, not ours
            logger.LogWarning("Domain exception occurred: {Code} - {Message}", de.Code, de.Message);

            var body = new
            {
                error = new
                {
                    type = "Domain",
                    code = de.Code,
                    message = de.Message
                }
            };
            ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Response.WriteAsJsonAsync(body);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled");
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}
