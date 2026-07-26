using System.Data;
using Microsoft.AspNetCore.Diagnostics;
using Shared.Exceptions;

namespace ShopApi.ExceptionHandlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case DuplicateNameException:
                httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                break;

            case BusinessException:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            default:
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                break;
        }

        await httpContext.Response.WriteAsJsonAsync(new
            {
                message = exception.Message
            }
        );

        return true;
    }
}