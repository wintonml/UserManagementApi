using System.Net;
using System.Text.Json;

namespace UserManagementAPI.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing request {Method} {Path}", context.Request.Method, context.Request.Path);

            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var payload = new { error = "Internal server error." };
                var json = JsonSerializer.Serialize(payload);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
