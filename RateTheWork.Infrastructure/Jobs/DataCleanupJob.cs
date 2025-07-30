using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Enums.VerificationRequest;
using RateTheWork.Infrastructure.Persistence;

namespace RateTheWork.Infrastructure.Jobs;

/// <summary>
/// Veri temizleme ve bakım job'ı
/// </summary>
public class DataCleanupJob
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataCleanupJob> _logger;
    private readonly IMetricsService _metricsService;

    public DataCleanupJob
    (
        ApplicationDbContext context
        , IMetricsService metricsService
        , ILogger<DataCleanupJob> logger
    )
    {
        _context = context;
        _metricsService = metricsService;
        _logger = logger;
    }

    /// <summary>
    /// Eski soft delete edilmiş kayıtları temizler
    /// </summary>
    [Queue("low")]
    [AutomaticRetry(Attempts = 3)]
    public async Task CleanupSoftDeletedRecordsAsync(int daysToKeep = 90)
    {
        try
        {
            _logger.LogInformation("Soft delete temizliği başlatılıyor. Gün limiti: {Days}", daysToKeep);

            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var deletedCount = 0;

            // Kullanıcılar
            var deletedUsers = await _context.Users
                .IgnoreQueryFilters()
                .Where(u => u.IsDeleted && u.DeletedAt < cutoffDate)
                .ToListAsync();

            if (deletedUsers.Any())
            {
                _context.Users.RemoveRange(deletedUsers);
                deletedCount += deletedUsers.Count;
                _logger.LogInformation("{Count} adet silinmiş kullanıcı temizlendi", deletedUsers.Count);
            }

            // İncelemeler
            var deletedReviews = await _context.Reviews
                .IgnoreQueryFilters()
                .Where(r => r.IsDeleted && r.DeletedAt < cutoffDate)
                .ToListAsync();

            if (deletedReviews.Any())
            {
                _context.Reviews.RemoveRange(deletedReviews);
                deletedCount += deletedReviews.Count;
                _logger.LogInformation("{Count} adet silinmiş inceleme temizlendi", deletedReviews.Count);
            }

            // İş ilanları
            var deletedJobPostings = await _context.JobPostings
                .IgnoreQueryFilters()
                .Where(j => j.IsDeleted && j.DeletedAt < cutoffDate)
                .ToListAsync();

            if (deletedJobPostings.Any())
            {
                _context.JobPostings.RemoveRange(deletedJobPostings);
                deletedCount += deletedJobPostings.Count;
                _logger.LogInformation("{Count} adet silinmiş iş ilanı temizlendi", deletedJobPostings.Count);
            }

            await _context.SaveChangesAsync();

            _metricsService.RecordCustomMetric("cleanup_soft_deleted", deletedCount, "records",
                new Dictionary<string, object?> { { "days_kept", daysToKeep } });

            _logger.LogInformation("Soft delete temizliği tamamlandı. Toplam temizlenen: {Count}", deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Soft delete temizliği sırasında hata oluştu");
            _metricsService.IncrementCounter("error", new Dictionary<string, object?>
            {
                { "type", "cleanup_soft_deleted" }
            });
            throw;
        }
    }

    /// <summary>
    /// Süresi dolmuş doğrulama taleplerini temizler
    /// </summary>
    [Queue("low")]
    public async Task CleanupExpiredVerificationRequestsAsync()
    {
        try
        {
            _logger.LogInformation("Süresi dolmuş doğrulama talepleri temizleniyor");

            var expiredRequests = await _context.VerificationRequests
                .Where(vr => vr.ExpiresAt < DateTime.UtcNow && vr.Status == VerificationRequestStatus.Pending)
                .ToListAsync();

            if (expiredRequests.Any())
            {
                foreach (var request in expiredRequests)
                {
                    request.Reject("SYSTEM", "Süre dolduğu için otomatik olarak iptal edildi");
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("{Count} adet süresi dolmuş doğrulama talebi güncellendi",
                    expiredRequests.Count);
                _metricsService.RecordCustomMetric("cleanup_expired_verifications", expiredRequests.Count, "requests");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Doğrulama talepleri temizliği sırasında hata oluştu");
            throw;
        }
    }

    /// <summary>
    /// Eski bildirim kayıtlarını temizler
    /// </summary>
    [Queue("low")]
    public async Task CleanupOldNotificationsAsync(int daysToKeep = 30)
    {
        try
        {
            _logger.LogInformation("Eski bildirimler temizleniyor. Gün limiti: {Days}", daysToKeep);

            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

            var oldNotifications = await _context.Notifications
                .Where(n => n.CreatedAt < cutoffDate && n.IsRead)
                .ToListAsync();

            if (oldNotifications.Any())
            {
                _context.Notifications.RemoveRange(oldNotifications);
                await _context.SaveChangesAsync();

                _logger.LogInformation("{Count} adet eski bildirim temizlendi", oldNotifications.Count);
                _metricsService.RecordCustomMetric("cleanup_old_notifications", oldNotifications.Count
                    , "notifications");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bildirim temizliği sırasında hata oluştu");
            throw;
        }
    }

    /// <summary>
    /// Eski audit log kayıtlarını temizler
    /// </summary>
    [Queue("low")]
    public async Task CleanupOldAuditLogsAsync(int daysToKeep = 180)
    {
        try
        {
            _logger.LogInformation("Eski audit logları temizleniyor. Gün limiti: {Days}", daysToKeep);

            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

            var oldLogs = await _context.AuditLogs
                .Where(a => a.CreatedAt < cutoffDate)
                .ToListAsync();

            if (oldLogs.Any())
            {
                _context.AuditLogs.RemoveRange(oldLogs);
                await _context.SaveChangesAsync();

                _logger.LogInformation("{Count} adet eski audit log temizlendi", oldLogs.Count);
                _metricsService.RecordCustomMetric("cleanup_old_audit_logs", oldLogs.Count, "logs");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audit log temizliği sırasında hata oluştu");
            throw;
        }
    }

    /// <summary>
    /// Süresi dolmuş iş ilanlarını kapatır
    /// </summary>
    [Queue("default")]
    public async Task CloseExpiredJobPostingsAsync()
    {
        try
        {
            _logger.LogInformation("Süresi dolmuş iş ilanları kapatılıyor");

            var expiredPostings = await _context.JobPostings
                .Where(j => j.ApplicationDeadline < DateTime.UtcNow && !j.IsDeleted)
                .ToListAsync();

            if (expiredPostings.Any())
            {
                // JobPosting entity'sinde IsActive property'si yok, soft delete yapalım
                foreach (var posting in expiredPostings)
                {
                    posting.SoftDelete("SYSTEM"); // Soft delete
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("{Count} adet süresi dolmuş iş ilanı kapatıldı", expiredPostings.Count);
                _metricsService.RecordCustomMetric("expired_job_postings_closed", expiredPostings.Count, "postings");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İş ilanı kapatma sırasında hata oluştu");
            throw;
        }
    }
}
