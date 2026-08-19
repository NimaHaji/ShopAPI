using System.Data;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Exceptions;

namespace ShopApi.ExceptionHandlers;

public class GlobalExceptionHandler : IExceptionHandler
{
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

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                message
            },
            cancellationToken
        );

        return true;
    }
}