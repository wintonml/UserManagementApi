using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace UserManagementAPI.Middleware;

public sealed class AuditMiddleware
{
    private readonly RequestDelegate _next;

    public AuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        // Read and buffer request body
        context.Request.EnableBuffering();
        string requestBody = string.Empty;
        if (context.Request.ContentLength > 0 && context.Request.Body.CanRead)
        {
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        // Swap response body to capture
        var originalBody = context.Response.Body;
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();

            // Read response
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            // Collect headers and redact sensitive ones
            var reqHeaders = context.Request.Headers.ToDictionary(h => h.Key, h => string.Join(';', h.Value.ToArray()));
            if (reqHeaders.ContainsKey("Authorization")) reqHeaders["Authorization"] = "REDACTED";

            var entry = new
            {
                Timestamp = DateTime.UtcNow,
                Request = new
                {
                    Method = context.Request.Method,
                    Path = context.Request.Path.Value,
                    QueryString = context.Request.QueryString.Value,
                    Headers = reqHeaders,
                    Body = requestBody
                },
                Response = new
                {
                    StatusCode = context.Response.StatusCode,
                    Body = responseText
                },
                DurationMs = sw.ElapsedMilliseconds
            };

            try
            {
                Directory.CreateDirectory("Logs");
                var json = JsonSerializer.Serialize(entry);
                await File.AppendAllTextAsync(Path.Combine("Logs", "audit.log"), json + Environment.NewLine);
            }
            catch
            {
                // Do not throw from logging
            }

            // Copy captured content back to original response body
            await responseBody.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
        }
    }
}
