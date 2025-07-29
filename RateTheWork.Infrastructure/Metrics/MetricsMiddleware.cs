using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using RateTheWork.Application.Common.Interfaces;

namespace RateTheWork.Infrastructure.Metrics;

/// <summary>
/// HTTP request metriklerini toplayan middleware
/// </summary>
public class MetricsMiddleware
{
    private readonly IMetricsService _metricsService;
    private readonly RequestDelegate _next;

    public MetricsMiddleware(RequestDelegate next, IMetricsService metricsService)
    {
        _next = next;
        _metricsService = metricsService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Health check endpoint'lerini hariç tut
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var path = context.Request.Path.Value ?? "unknown";
        var method = context.Request.Method;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Request metriklerini kaydet
            var tags = new Dictionary<string, object?>
            {
                { "method", method }, { "endpoint", NormalizeEndpoint(path) }
                , { "status_code", context.Response.StatusCode }
                , { "status_class", $"{context.Response.StatusCode / 100}xx" }
            };

            // Request sayacını artır
            _metricsService.IncrementCounter("request", tags);

            // Request süresini kaydet
            _metricsService.RecordDuration("request", stopwatch.Elapsed.TotalSeconds, tags);

            // Hata durumunda error counter'ı artır
            if (context.Response.StatusCode >= 400)
            {
                _metricsService.IncrementCounter("error", new Dictionary<string, object?>
                {
                    { "type", context.Response.StatusCode >= 500 ? "server_error" : "client_error" }
                    , { "status_code", context.Response.StatusCode }, { "endpoint", NormalizeEndpoint(path) }
                });
            }
        }
    }

    /// <summary>
    /// Endpoint'i normalize eder (parametreleri kaldırır)
    /// </summary>
    private string NormalizeEndpoint(string path)
    {
        // GUID pattern'ini {id} ile değiştir
        var normalized = Regex.Replace(
            path,
            @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
            "{id}");

        // Sayısal ID'leri {id} ile değiştir
        normalized = Regex.Replace(
            normalized,
            @"\/\d+",
            "/{id}");

        return normalized.ToLowerInvariant();
    }
}
