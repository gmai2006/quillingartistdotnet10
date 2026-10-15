using ApiService.Security;

namespace ApiService.Endpoints;

internal static class EndpointHelpers
{
    public static IResult Unauthorized() =>
        Results.Text("Authentication required.", statusCode: StatusCodes.Status401Unauthorized);

    public static IResult BadRequest(string message) => Results.BadRequest(new { error = message });

    public static IResult Csv(string csv, string fileName) =>
        Results.Text(csv, "text/csv").WithHeader("Content-Disposition", $"attachment; filename={fileName}");

    public static async Task<IResult> Authorized(AuthContext auth, Func<Task<IResult>> handler)
    {
        if (!auth.IsAuthenticated) return Unauthorized();
        try
        {
            return await handler();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private static IResult WithHeader(this IResult result, string name, string value) =>
        new HeaderResult(result, name, value);

    private sealed class HeaderResult(IResult inner, string name, string value) : IResult
    {
        public Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.Headers[name] = value;
            return inner.ExecuteAsync(httpContext);
        }
    }
}
