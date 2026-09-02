using System.Diagnostics;
using System.Security.Claims;
using Serilog.Context;

namespace ShopApi.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        var traceId =
            Activity.Current?.TraceId.ToString()
            ?? context.TraceIdentifier;

        var userId = context.User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        var isAuthenticated =
            context.User.Identity?.IsAuthenticated ?? false;

        using var traceScope =
            LogContext.PushProperty("TraceId", traceId);

        using var methodScope =
            LogContext.PushProperty(
                "Method",
                context.Request.Method);

        using var pathScope =
            LogContext.PushProperty(
                "Path",
                context.Request.Path);

        using var userScope =
            LogContext.PushProperty(
                "UserId",
                userId ?? "Anonymous");

        using var authScope =
            LogContext.PushProperty(
                "IsAuthenticated",
                isAuthenticated);

        _logger.LogInformation("HTTP request started.");

        try
        {
            await _next(context);
        }
        catch
        {
            throw;
        }

        stopwatch.Stop();

        var statusCode = context.Response.StatusCode;
        var durationMs = stopwatch.ElapsedMilliseconds;

        if (statusCode >= 500)
        {
            _logger.LogError(
                "HTTP request completed with server error. " +
                "StatusCode: {StatusCode}, DurationMs: {DurationMs}",
                statusCode,
                durationMs);
        }
        else if (statusCode >= 400)
        {
            _logger.LogWarning(
                "HTTP request completed with client error. " +
                "StatusCode: {StatusCode}, DurationMs: {DurationMs}",
                statusCode,
                durationMs);
        }
        else
        {
            _logger.LogInformation(
                "HTTP request completed. " +
                "StatusCode: {StatusCode}, DurationMs: {DurationMs}",
                statusCode,
                durationMs);
        }
    }
}