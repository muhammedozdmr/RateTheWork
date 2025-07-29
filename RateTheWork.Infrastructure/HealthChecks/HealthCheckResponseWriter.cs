using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace RateTheWork.Infrastructure.HealthChecks;

public static class HealthCheckResponseWriter
{
    public static async Task WriteResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var options = new JsonSerializerOptions
        {
            WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var response = new
        {
            status = report.Status.ToString(), totalDuration = report.TotalDuration.TotalMilliseconds
            , timestamp = DateTime.UtcNow, checks = report.Entries.Select(entry => new
            {
                name = entry.Key, status = entry.Value.Status.ToString()
                , duration = entry.Value.Duration.TotalMilliseconds, description = entry.Value.Description
                , exception = entry.Value.Exception?.Message, data = entry.Value.Data
            })
        };

        var json = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(json);
    }
}
