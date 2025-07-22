using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Events.JobPosting;

namespace RateTheWork.Application.Features.JobPostings.EventHandlers;

/// <summary>
/// İş ilanı yayınlandı event handler
/// </summary>
public class JobPostingPublishedEventHandler : INotificationHandler<JobPostingPublishedEvent>
{
    private readonly ICacheService _cache;
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<JobPostingPublishedEventHandler> _logger;
    private readonly IPushNotificationService _pushNotificationService;

    public JobPostingPublishedEventHandler
    (
        ILogger<JobPostingPublishedEventHandler> logger
        , IApplicationDbContext context
        , IEmailService emailService
        , ICacheService cache
        , IPushNotificationService pushNotificationService
    )
    {
        _logger = logger;
        _context = context;
        _emailService = emailService;
        _cache = cache;
        _pushNotificationService = pushNotificationService;
    }

    public async Task Handle(JobPostingPublishedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling job posting published event for {JobPostingId}", notification.JobPostingId);

        // İlgili kullanıcılara bildirim gönder
        await NotifyInterestedUsersAsync(notification, cancellationToken);

        // Cache'i temizle
        await InvalidateCacheAsync(notification, cancellationToken);

        // SEO için sitemap güncelle
        await UpdateSitemapAsync(notification, cancellationToken);

        // İstatistikleri güncelle
        await UpdateStatisticsAsync(notification, cancellationToken);
    }

    private async Task NotifyInterestedUsersAsync
        (JobPostingPublishedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // İş ilanını getir
            var jobPosting = await _context.JobPostings
                .Include(jp => jp.Company)
                .FirstOrDefaultAsync(jp => jp.Id == notification.JobPostingId, cancellationToken);

            if (jobPosting == null) return;

            // İlgilenen kullanıcıları bul (job alert'i olan kullanıcılar)
            var interestedUsers = await _context.Users
                .Where(u => u.IsActive &&
                            u.JobAlerts.Any(ja =>
                                ja.IsActive &&
                                (ja.Keywords.Contains(jobPosting.Title) ||
                                 ja.City == jobPosting.City ||
                                 ja.JobType == jobPosting.JobType)))
                .Take(1000) // Performans için limit
                .ToListAsync(cancellationToken);

            // Toplu bildirim gönder
            var notificationTasks = interestedUsers.Select(user =>
                SendJobAlertNotificationAsync(user, jobPosting, cancellationToken));

            await Task.WhenAll(notificationTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify interested users for job posting {JobPostingId}"
                , notification.JobPostingId);
        }
    }

    private async Task SendJobAlertNotificationAsync
    (
        User user
        , JobPosting jobPosting
        , CancellationToken cancellationToken
    )
    {
        try
        {
            // E-posta bildirimi
            if (user.IsEmailVerified)
            {
                await _emailService.SendEmailAsync(
                    to: user.Email,
                    subject: $"Yeni İş İlanı: {jobPosting.Title}",
                    template: "job.alert.new_posting",
                    model: new
                    {
                        UserName = user.AnonymousUsername, JobTitle = jobPosting.Title
                        , CompanyName = jobPosting.Company?.Name, City = jobPosting.City
                        , JobType = jobPosting.JobType.ToString(), ViewUrl = $"/jobs/{jobPosting.Id}"
                    },
                    cancellationToken);
            }

            // Push notification
            await _pushNotificationService.SendAsync(
                userId: user.Id,
                title: "Yeni İş İlanı",
                message: $"{jobPosting.Company?.Name} firması {jobPosting.Title} pozisyonu için arama yapıyor",
                data: new Dictionary<string, string>
                {
                    ["jobPostingId"] = jobPosting.Id, ["companyId"] = jobPosting.CompanyId
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send job alert to user {UserId}", user.Id);
        }
    }

    private async Task InvalidateCacheAsync(JobPostingPublishedEvent notification, CancellationToken cancellationToken)
    {
        var cacheKeys = new[]
        {
            $"JobPostings:Company:{notification.CompanyId}", $"JobPostings:Latest"
            , $"JobPostings:Count:{notification.CompanyId}", "Statistics:JobPostings"
        };

        foreach (var key in cacheKeys)
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
    }

    private Task UpdateSitemapAsync(JobPostingPublishedEvent notification, CancellationToken cancellationToken)
    {
        // Sitemap güncelleme - background job olarak işlenebilir
        _logger.LogInformation("Updating sitemap for new job posting {JobPostingId}", notification.JobPostingId);
        return Task.CompletedTask;
    }

    private async Task UpdateStatisticsAsync(JobPostingPublishedEvent notification, CancellationToken cancellationToken)
    {
        // İstatistikleri güncelle
        var statsKey = $"Statistics:Company:{notification.CompanyId}:JobPostings";
        var currentStats = await _cache.GetAsync<Dictionary<string, int>>(statsKey, cancellationToken)
                           ?? new Dictionary<string, int>();

        var month = DateTime.UtcNow.ToString("yyyy-MM");
        currentStats[month] = currentStats.GetValueOrDefault(month, 0) + 1;

        await _cache.SetAsync(statsKey, currentStats, TimeSpan.FromDays(30), cancellationToken);
    }
}
