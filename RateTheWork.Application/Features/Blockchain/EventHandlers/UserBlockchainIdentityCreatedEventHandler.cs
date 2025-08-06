using MediatR;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Events.User;

namespace RateTheWork.Application.Features.Blockchain.EventHandlers;

public sealed class UserBlockchainIdentityCreatedEventHandler : INotificationHandler<UserBlockchainIdentityCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly IMetricsService _metricsService;
    private readonly ILogger<UserBlockchainIdentityCreatedEventHandler> _logger;

    public UserBlockchainIdentityCreatedEventHandler(
        IEmailService emailService,
        IMetricsService metricsService,
        ILogger<UserBlockchainIdentityCreatedEventHandler> logger)
    {
        _emailService = emailService;
        _metricsService = metricsService;
        _logger = logger;
    }

    public async Task Handle(UserBlockchainIdentityCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Metrikleri güncelle
            await _metricsService.IncrementCounterAsync("blockchain.identity.created", 1);

            // Log
            _logger.LogInformation("Blockchain identity created for user {UserId} with wallet {WalletAddress}",
                notification.UserId, notification.WalletAddress);

            // İsteğe bağlı: Kullanıcıya bilgilendirme e-postası gönder
            // await _emailService.SendAsync(...);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling UserBlockchainIdentityCreatedEvent for user {UserId}",
                notification.UserId);
            // Event handler'da hata olsa bile akışı kesmemeliyiz
        }
    }
}