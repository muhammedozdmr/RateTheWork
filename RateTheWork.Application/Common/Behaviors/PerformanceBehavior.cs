using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace RateTheWork.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior - Yavaş çalışan request'leri tespit eder ve loglar
/// </summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly Stopwatch _timer;
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _timer = new Stopwatch();
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        // 500ms'den uzun süren request'leri logla
        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogWarning(
                "Long Running Request: {RequestName} ({ElapsedMilliseconds} milliseconds) {@Request}",
                requestName, elapsedMilliseconds, request);

            // Çok uzun süren request'ler için ekstra detay
            if (elapsedMilliseconds > 1000)
            {
                _logger.LogError(
                    "CRITICAL Performance Issue: {RequestName} took {ElapsedMilliseconds}ms which is over 1 second!",
                    requestName, elapsedMilliseconds);
            }
        }

        return response;
    }
}
