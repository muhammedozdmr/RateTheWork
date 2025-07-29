using MediatR;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Enums.Subscription;
using RateTheWork.Domain.Events.Subscription;

namespace RateTheWork.Application.Features.Subscriptions.EventHandlers;

/// <summary>
/// Üyelik oluşturuldu event handler
/// </summary>
public class SubscriptionCreatedEventHandler : INotificationHandler<SubscriptionCreatedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<SubscriptionCreatedEventHandler> _logger;
    private readonly IPushNotificationService _pushNotificationService;

    public SubscriptionCreatedEventHandler
    (
        ILogger<SubscriptionCreatedEventHandler> logger
        , IEmailService emailService
        , IPushNotificationService pushNotificationService
        , IApplicationDbContext context
    )
    {
        _logger = logger;
        _emailService = emailService;
        _pushNotificationService = pushNotificationService;
        _context = context;
    }

    public async Task Handle(SubscriptionCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling subscription created event for user {UserId}", notification.UserId);

        // Hoşgeldin e-postası gönder
        await SendWelcomeEmailAsync(notification, cancellationToken);

        // Push notification gönder
        await SendPushNotificationAsync(notification, cancellationToken);

        // Analytics kaydı oluştur
        await RecordAnalyticsAsync(notification, cancellationToken);

        // Eğer trial ise, trial bitiş hatırlatıcısı kur
        if (notification.TrialEndDate.HasValue)
        {
            await ScheduleTrialEndReminderAsync(notification, cancellationToken);
        }
    }

    private async Task SendWelcomeEmailAsync(SubscriptionCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users.FindAsync(new object[] { notification.UserId }, cancellationToken);
            if (user == null) return;

            var emailTemplate = notification.Type switch
            {
                SubscriptionType.Free => "subscription.welcome.free"
                , SubscriptionType.CompanyBasic => "subscription.welcome.basic"
                , SubscriptionType.CompanyProfessional => "subscription.welcome.professional"
                , SubscriptionType.CompanyEnterprise => "subscription.welcome.enterprise"
                , SubscriptionType.IndividualPremium => "subscription.welcome.individual", _ => "subscription.welcome.default"
            };

            await _emailService.SendEmailAsync(
                to: user.Email,
                subject: "RateTheWork'e Hoş Geldiniz!",
                body: $"Merhaba {user.AnonymousUsername},\n\nRateTheWork ailesine hoş geldiniz!",
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email for subscription {SubscriptionId}"
                , notification.SubscriptionId);
        }
    }

    private async Task SendPushNotificationAsync
        (SubscriptionCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            await _pushNotificationService.SendAsync(
                userId: notification.UserId,
                message: $"{notification.Type} üyeliğiniz aktif edildi. Tüm özelliklerin keyfini çıkarın!",
                data: new Dictionary<string, string>
                {
                    ["subscriptionId"] = notification.SubscriptionId, ["type"] = notification.Type.ToString()
                },
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification for subscription {SubscriptionId}"
                , notification.SubscriptionId);
        }
    }

    private Task RecordAnalyticsAsync(SubscriptionCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Analytics kaydı - background job olarak işlenebilir
        _logger.LogInformation("Recording analytics for new subscription {SubscriptionId} of type {Type}",
            notification.SubscriptionId, notification.Type);

        return Task.CompletedTask;
    }

    private Task ScheduleTrialEndReminderAsync
        (SubscriptionCreatedEvent notification, CancellationToken cancellationToken)
    {
        if (!notification.TrialEndDate.HasValue)
            return Task.CompletedTask;

        // Trial bitiş hatırlatıcısı - background job scheduler ile kurulabilir
        var reminderDates = new[]
        {
            notification.TrialEndDate.Value.AddDays(-7), // 7 gün kala
            notification.TrialEndDate.Value.AddDays(-3)
            , // 3 gün kala
            notification.TrialEndDate.Value.AddDays(-1) // 1 gün kala
        };

        _logger.LogInformation("Scheduling trial end reminders for subscription {SubscriptionId}"
            , notification.SubscriptionId);

        return Task.CompletedTask;
    }
}
