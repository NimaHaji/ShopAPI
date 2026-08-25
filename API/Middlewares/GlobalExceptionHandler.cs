using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics;
using Serilog.Context;
using Shared.Exceptions;

namespace ShopApi.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            DuplicateNameException => StatusCodes.Status409Conflict,
            BusinessException => StatusCodes.Status400BadRequest,
            InsufficientStockException => StatusCodes.Status409Conflict,
            ForbiddenAccessException => StatusCodes.Status403Forbidden,
            NotFoundException => StatusCodes.Status404NotFound,
            CartEmptyException => StatusCodes.Status400BadRequest,
            InvalidQuantityException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            ConflictException => StatusCodes.Status409Conflict,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        var message = exception switch
        {
            DuplicateNameException => exception.Message,
            BusinessException => exception.Message,
            InsufficientStockException => exception.Message,
            ForbiddenAccessException => exception.Message,
            NotFoundException => exception.Message,
            CartEmptyException => exception.Message,
            InvalidQuantityException => exception.Message,
            UnauthorizedAccessException => exception.Message,
            ConflictException => exception.Message,
            InvalidOperationException => exception.Message,
            _ => "خطای غیرمنتظره‌ای در سرور رخ داده است. لطفاً بعداً دوباره تلاش کنید."
        };

        var traceId =
            Activity.Current?.TraceId.ToString()
            ?? httpContext.TraceIdentifier;

        var userId =
            httpContext.User.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ?? "Anonymous";

        var method = httpContext.Request.Method;
        var path = httpContext.Request.Path;

        using var traceScope =
            LogContext.PushProperty("TraceId", traceId);

        using var methodScope =
            LogContext.PushProperty("Method", method);

        using var pathScope =
            LogContext.PushProperty("Path", path);

        using var userScope =
            LogContext.PushProperty("UserId", userId);

        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred. " +
                "ExceptionType: {ExceptionType}, " +
                "StatusCode: {StatusCode}",
                exception.GetType().Name,
                statusCode);
        }
        else
        {
            _logger.LogWarning(
                "Request failed. " +
                "ExceptionType: {ExceptionType}, " +
                "StatusCode: {StatusCode}, " +
                "Message: {Message}",
                exception.GetType().Name,
                statusCode,
                exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                message
            },
            cancellationToken);

        return true;
    }
}