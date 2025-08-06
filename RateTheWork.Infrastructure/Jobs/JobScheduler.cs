using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RateTheWork.Infrastructure.Persistence;

namespace RateTheWork.Infrastructure.Jobs;

/// <summary>
/// Hangfire job'larını zamanlamak için hosted service
/// </summary>
public class JobScheduler : IHostedService
{
    private readonly ILogger<JobScheduler> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public JobScheduler(IServiceProvider serviceProvider, ILogger<JobScheduler> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Job zamanlayıcı başlatılıyor...");

        // Hangfire devre dışıysa job'ları zamanlamayı atla
        var disableHangfire = _configuration.GetValue<bool>("DisableHangfire");
        if (disableHangfire)
        {
            _logger.LogInformation("Hangfire devre dışı, job'lar zamanlanmıyor");
            return Task.CompletedTask;
        }

        // Tekrarlayan job'ları zamanla
        ScheduleRecurringJobs();

        // Tek seferlik job'ları zamanla
        ScheduleOneTimeJobs();

        _logger.LogInformation("Job zamanlayıcı başarıyla başlatıldı");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Job zamanlayıcı durduruluyor...");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Tekrarlayan job'ları zamanlar
    /// </summary>
    private void ScheduleRecurringJobs()
    {
        // Veri temizleme job'ları
        RecurringJob.AddOrUpdate<DataCleanupJob>(
            "temizlik-soft-deleted-kayitlar",
            job => job.CleanupSoftDeletedRecordsAsync(90),
            Cron.Daily(3, 0), // Her gün saat 03:00'te
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time")
            });

        RecurringJob.AddOrUpdate<DataCleanupJob>(
            "temizlik-suresi-dolmus-dogrulamalar",
            job => job.CleanupExpiredVerificationRequestsAsync(),
            Cron.Hourly); // Her saat başı

        RecurringJob.AddOrUpdate<DataCleanupJob>(
            "temizlik-eski-bildirimler",
            job => job.CleanupOldNotificationsAsync(30),
            Cron.Daily(4, 0)); // Her gün saat 04:00'te

        RecurringJob.AddOrUpdate<DataCleanupJob>(
            "temizlik-eski-audit-loglar",
            job => job.CleanupOldAuditLogsAsync(180),
            Cron.Weekly(DayOfWeek.Sunday, 2, 0)); // Her pazar saat 02:00'de

        RecurringJob.AddOrUpdate<DataCleanupJob>(
            "kapat-suresi-dolmus-ilanlar",
            job => job.CloseExpiredJobPostingsAsync(),
            Cron.Daily(0, 0)); // Her gün gece yarısı

        // Rapor oluşturma job'ları
        RecurringJob.AddOrUpdate<ReportGenerationJob>(
            "haftalik-sistem-raporu",
            job => job.GenerateWeeklySystemReportAsync(),
            Cron.Weekly(DayOfWeek.Monday, 9, 0), // Her pazartesi sabah 09:00'da
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time")
            });

        // Şirket raporları - Her ayın ilk günü
        RecurringJob.AddOrUpdate(
            "aylik-sirket-raporlari",
            () => ScheduleMonthlyCompanyReports(),
            Cron.Monthly(1, 2, 0), // Her ayın 1'i saat 02:00'de
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time")
            });

        _logger.LogInformation("Tekrarlayan job'lar zamanlandı");
    }

    /// <summary>
    /// Tek seferlik job'ları zamanlar
    /// </summary>
    private void ScheduleOneTimeJobs()
    {
        // İlk kurulumda veya gerektiğinde tek seferlik job'lar eklenebilir
        _logger.LogInformation("Tek seferlik job'lar kontrol edildi");
    }

    /// <summary>
    /// Tüm aktif şirketler için aylık rapor job'larını zamanlar
    /// </summary>
    [Queue("low")]
    public static void ScheduleMonthlyCompanyReports()
    {
        using var scope = JobStorage.Current.GetConnection();

        // Bu metod Hangfire context'inde çalışacağı için DI kullanamıyoruz
        // Bunun yerine bir batch job oluşturabiliriz
        BackgroundJob.Enqueue<MonthlyReportSchedulerJob>(job => job.ScheduleAllCompanyReportsAsync());
    }
}

/// <summary>
/// Aylık şirket raporlarını zamanlamak için yardımcı job
/// </summary>
public class MonthlyReportSchedulerJob
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MonthlyReportSchedulerJob> _logger;

    public MonthlyReportSchedulerJob(ApplicationDbContext context, ILogger<MonthlyReportSchedulerJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    [Queue("low")]
    public async Task ScheduleAllCompanyReportsAsync()
    {
        _logger.LogInformation("Aylık şirket raporları zamanlanıyor...");

        var activeCompanies = await _context.Companies
            .Where(c => c.IsActive)
            .Select(c => c.Id)
            .ToListAsync();

        foreach (var companyId in activeCompanies)
        {
            // Her şirket için ayrı job oluştur
            BackgroundJob.Enqueue<ReportGenerationJob>(
                job => job.GenerateMonthlyCompanyReportAsync(Guid.Parse(companyId)));
        }

        _logger.LogInformation("{Count} adet şirket için aylık rapor zamanlandı", activeCompanies.Count);
    }
}
