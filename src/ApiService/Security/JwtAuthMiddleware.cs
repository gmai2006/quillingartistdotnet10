using System.Text;
using ApiService.Configuration;
using ApiService.Dao;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ApiService.Security;

public class JwtAuthMiddleware(RequestDelegate next, IOptions<JwtOptions> options, ILogger<JwtAuthMiddleware> logger)
{
    private const string BearerPrefix = "Bearer ";
    private static readonly JsonWebTokenHandler Handler = new();

    private readonly TokenValidationParameters _validation = new()
    {
        ValidIssuer = options.Value.Issuer,
        ValidAudience = options.Value.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.SigningKey)),
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        RequireSignedTokens = true,
        RequireExpirationTime = true,
        ClockSkew = TimeSpan.FromMinutes(1),
    };

    public async Task InvokeAsync(HttpContext context, AuthContext authContext, UsersDao usersDao)
    {
        string? authHeader = context.Request.Headers.Authorization;

        if (authHeader is null || !authHeader.StartsWith(BearerPrefix, StringComparison.Ordinal))
        {
            await next(context);
            return;
        }

        var rawToken = authHeader[BearerPrefix.Length..].Trim();

        var result = await Handler.ValidateTokenAsync(rawToken, _validation);
        if (!result.IsValid || result.SecurityToken is not JsonWebToken jwt)
        {
            logger.LogInformation("Rejected bearer token: {Reason}", result.Exception?.Message);
            await AbortAsync(context, "Invalid or expired token");
            return;
        }

        var email = Claim(jwt, "email") ?? Claim(jwt, "preferred_username") ?? jwt.Subject;
        if (string.IsNullOrEmpty(email))
        {
            await AbortAsync(context, "Token does not contain email");
            return;
        }

        var user = await usersDao.GetByEmailAsync(email);
        if (user is null)
        {
            await AbortAsync(context, "No matching user found");
            return;
        }
        authContext.Actor = user;

        await next(context);
    }

    private static string? Claim(JsonWebToken jwt, string name) =>
        jwt.TryGetPayloadValue<string>(name, out var value) ? value : null;

    private static async Task AbortAsync(HttpContext context, string msg)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(msg);
    }
}
