using MediatR;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Events.Review;

namespace RateTheWork.Application.Features.Blockchain.EventHandlers;

public sealed class ReviewStoredOnBlockchainEventHandler : INotificationHandler<ReviewStoredOnBlockchainEvent>
{
    private readonly ICacheService _cacheService;
    private readonly IMetricsService _metricsService;
    private readonly ILogger<ReviewStoredOnBlockchainEventHandler> _logger;

    public ReviewStoredOnBlockchainEventHandler(
        ICacheService cacheService,
        IMetricsService metricsService,
        ILogger<ReviewStoredOnBlockchainEventHandler> logger)
    {
        _cacheService = cacheService;
        _metricsService = metricsService;
        _logger = logger;
    }

    public async Task Handle(ReviewStoredOnBlockchainEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Cache'i invalidate et
            var cacheKey = $"review:{notification.ReviewId}";
            await _cacheService.RemoveAsync(cacheKey, cancellationToken);
            
            // Company review cache'ini de temizle
            var companyCacheKey = $"company:reviews:{notification.CompanyId}";
            await _cacheService.RemoveAsync(companyCacheKey, cancellationToken);

            // Metrikleri güncelle
            await _metricsService.IncrementCounterAsync("blockchain.review.stored", 1);

            // Log
            _logger.LogInformation("Review {ReviewId} stored on blockchain with transaction {TransactionHash}",
                notification.ReviewId, notification.TransactionHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ReviewStoredOnBlockchainEvent for review {ReviewId}",
                notification.ReviewId);
            // Event handler'da hata olsa bile akışı kesmemeliyiz
        }
    }
}