using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ShopApi.HealthChecks;

public static class HealthCheckResponseWriter
{
    public static async Task WriteResponseAsync(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration,
            checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    duration = entry.Value.Duration,
                    error = entry.Value.Exception?.Message
                })
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}