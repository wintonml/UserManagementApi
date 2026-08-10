using System.Text.Json;

namespace UserManagementAPI.Middleware;

public sealed class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;
    private readonly HashSet<string> _validTokens;

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;

        var tokenList = configuration["Auth:ValidTokens"] ?? "dev-token";
        _validTokens = tokenList
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.Ordinal);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/openapi", StringComparison.OrdinalIgnoreCase) ||
            context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await ReturnUnauthorized(context, "Missing or malformed Authorization header.");
            return;
        }

        var token = authHeader[7..].Trim();
        if (!_validTokens.Contains(token))
        {
            _logger.LogWarning("Invalid token for request {Method} {Path}", context.Request.Method, context.Request.Path);
            await ReturnUnauthorized(context, "Invalid token.");
            return;
        }

        await _next(context);
    }

    private static Task ReturnUnauthorized(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
    }
}
