using MediatR;
using Microsoft.Extensions.Logging;

namespace RateTheWork.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior - Tüm request'leri loglar
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestGuid = Guid.NewGuid().ToString();

        // Request'i logla
        _logger.LogInformation(
            "Handling {RequestName} [{RequestGuid}] {@Request}",
            requestName, requestGuid, request);

        TResponse response;
        
        try
        {
            response = await next();
        }
        catch (Exception ex)
        {
            // Hata durumunu logla
            _logger.LogError(ex,
                "Request {RequestName} [{RequestGuid}] failed with error: {ErrorMessage}",
                requestName, requestGuid, ex.Message);
            throw;
        }

        // Response'u logla
        _logger.LogInformation(
            "Handled {RequestName} [{RequestGuid}] - Response: {@Response}",
            requestName, requestGuid, response);

        return response;
    }
}
