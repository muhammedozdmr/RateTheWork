using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace RateTheWork.Infrastructure.Metrics;

/// <summary>
/// OpenTelemetry yapılandırması
/// </summary>
public static class OpenTelemetryConfiguration
{
    /// <summary>
    /// OpenTelemetry servislerini yapılandırır
    /// </summary>
    public static IServiceCollection AddOpenTelemetryConfiguration(this IServiceCollection services)
    {
        var serviceName = "RateTheWork.API";
        var serviceVersion = "1.0.0";

        // Resource bilgilerini oluştur
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
            .AddAttributes(new Dictionary<string, object>
            {
                ["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
                , ["deployment.environment"] = Environment.GetEnvironmentVariable("DEPLOYMENT_ENV") ?? "local"
                , ["service.namespace"] = "ratethework", ["service.instance.id"] = Environment.MachineName
            });

        // Tracing yapılandırması
        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // HTTP isteklerini izle
                        options.RecordException = true;
                        options.Filter = (httpContext) =>
                        {
                            // Health check endpoint'lerini hariç tut
                            var path = httpContext.Request.Path.Value;
                            return !path?.StartsWith("/health") ?? true;
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        // Dış HTTP çağrılarını izle
                        options.RecordException = true;
                    })
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        // EF Core sorgularını izle
                        options.SetDbStatementForText = true;
                        options.SetDbStatementForStoredProcedure = true;
                    })
                    .AddSource("RateTheWork.*") // Custom trace source'ları ekle
                    .AddSource("Hangfire.*"); // Hangfire trace'lerini ekle

                // Production'da OTLP exporter ekle
                if (Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") != null)
                {
                    tracing.AddOtlpExporter();
                }
                else
                {
                    // Development'ta console exporter kullan
                    tracing.AddConsoleExporter();
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation() // ASP.NET Core metrikleri
                    .AddHttpClientInstrumentation() // HTTP client metrikleri
                    .AddRuntimeInstrumentation() // .NET runtime metrikleri
                    .AddProcessInstrumentation() // Process metrikleri
                    .AddMeter("RateTheWork.Application") // Custom meter'ımız
                    .AddMeter("Hangfire"); // Hangfire metrikleri

                // Histogram bucket'larını özelleştir
                metrics.AddView("http.server.request.duration",
                    new ExplicitBucketHistogramConfiguration
                    {
                        Boundaries = new double[] { 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10 }
                    });

                // Production'da OTLP exporter ekle
                if (Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") != null)
                {
                    metrics.AddOtlpExporter();
                }
                else
                {
                    // Development'ta console exporter kullan
                    metrics.AddConsoleExporter();
                }
            });

        return services;
    }

    /// <summary>
    /// Logging için OpenTelemetry yapılandırması
    /// </summary>
    public static ILoggingBuilder AddOpenTelemetryLogging(this ILoggingBuilder logging)
    {
        logging.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("RateTheWork.API", "1.0.0"));

            // Production'da OTLP exporter ekle
            if (Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") != null)
            {
                options.AddOtlpExporter();
            }
            else
            {
                // Development'ta console exporter kullan
                options.AddConsoleExporter();
            }
        });

        return logging;
    }
}
