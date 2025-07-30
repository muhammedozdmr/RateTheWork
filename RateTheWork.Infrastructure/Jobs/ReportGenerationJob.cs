using System.Text;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.Review;
using RateTheWork.Infrastructure.Persistence;

namespace RateTheWork.Infrastructure.Jobs;

/// <summary>
/// Rapor oluşturma job'ı
/// </summary>
public class ReportGenerationJob
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<ReportGenerationJob> _logger;
    private readonly IMetricsService _metricsService;

    public ReportGenerationJob
    (
        ApplicationDbContext context
        , IEmailService emailService
        , IFileStorageService fileStorageService
        , IMetricsService metricsService
        , ILogger<ReportGenerationJob> logger
    )
    {
        _context = context;
        _emailService = emailService;
        _fileStorageService = fileStorageService;
        _metricsService = metricsService;
        _logger = logger;
    }

    /// <summary>
    /// Aylık şirket raporu oluşturur
    /// </summary>
    [Queue("default")]
    [AutomaticRetry(Attempts = 3)]
    public async Task GenerateMonthlyCompanyReportAsync(Guid companyId)
    {
        try
        {
            _logger.LogInformation("Aylık şirket raporu oluşturuluyor: {CompanyId}", companyId);

            var company = await _context.Companies
                .Include(c => c.Branches)
                .FirstOrDefaultAsync(c => c.Id == companyId.ToString());

            if (company == null)
            {
                _logger.LogWarning("Şirket bulunamadı: {CompanyId}", companyId);
                return;
            }

            var startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // İstatistikleri topla
            var stats = await GatherCompanyStatisticsAsync(companyId, startDate, endDate);

            // Rapor içeriğini oluştur
            var reportContent = GenerateReportContent(company, stats, startDate, endDate);

            // Raporu kaydet
            var fileName = $"company-report-{company.TaxId}-{startDate:yyyy-MM}.csv";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(reportContent));
            var fileUrl = await _fileStorageService.UploadFileAsync(stream, fileName, "text/csv", "reports");

            // Rapor kaydını oluştur
            var report = SystemReport.Create(
                "MonthlyCompanyReport",
                $"{company.Name} - {startDate:MMMM yyyy} Raporu",
                $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy} dönemi şirket raporu",
                new Dictionary<string, object>
                {
                    { "companyId", companyId }, { "startDate", startDate }, { "endDate", endDate }
                },
                Guid.Empty // System generated
            );
            report.MarkAsCompleted(fileUrl);

            _context.SystemReports.Add(report);
            await _context.SaveChangesAsync();

            // Email gönder
            await SendReportEmailAsync(company, fileUrl, startDate);

            _metricsService.IncrementCounter("report_generated", new Dictionary<string, object?>
            {
                { "type", "monthly_company" }
            });

            _logger.LogInformation("Aylık şirket raporu başarıyla oluşturuldu: {CompanyId}", companyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aylık şirket raporu oluşturulurken hata: {CompanyId}", companyId);
            throw;
        }
    }

    /// <summary>
    /// Haftalık sistem raporu oluşturur
    /// </summary>
    [Queue("low")]
    public async Task GenerateWeeklySystemReportAsync()
    {
        try
        {
            _logger.LogInformation("Haftalık sistem raporu oluşturuluyor");

            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-7);

            var stats = new SystemStatistics
            {
                TotalUsers = await _context.Users.CountAsync()
                , NewUsers = await _context.Users.CountAsync(u => u.CreatedAt >= startDate)
                , TotalCompanies = await _context.Companies.CountAsync()
                , NewCompanies = await _context.Companies.CountAsync(c => c.CreatedAt >= startDate)
                , TotalReviews = await _context.Reviews.CountAsync()
                , NewReviews = await _context.Reviews.CountAsync(r => r.CreatedAt >= startDate)
                , TotalJobPostings = await _context.JobPostings.CountAsync()
                , NewJobPostings = await _context.JobPostings.CountAsync(j => j.CreatedAt >= startDate)
                , TotalApplications = await _context.CVApplications.CountAsync()
                , NewApplications = await _context.CVApplications.CountAsync(a => a.CreatedAt >= startDate)
            };

            var reportContent = GenerateSystemReportContent(stats, startDate, endDate);

            // Raporu kaydet
            var fileName = $"system-report-{startDate:yyyy-MM-dd}.csv";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(reportContent));
            var fileUrl = await _fileStorageService.UploadFileAsync(stream, fileName, "text/csv", "reports");

            // Rapor kaydını oluştur
            var report = SystemReport.Create(
                "WeeklySystemReport",
                $"Haftalık Sistem Raporu - {startDate:dd.MM.yyyy}",
                $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy} dönemi sistem raporu",
                new Dictionary<string, object>
                {
                    { "startDate", startDate }, { "endDate", endDate }
                },
                Guid.Empty
            );
            report.MarkAsCompleted(fileUrl);

            _context.SystemReports.Add(report);
            await _context.SaveChangesAsync();

            _metricsService.IncrementCounter("report_generated", new Dictionary<string, object?>
            {
                { "type", "weekly_system" }
            });

            _logger.LogInformation("Haftalık sistem raporu başarıyla oluşturuldu");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Haftalık sistem raporu oluşturulurken hata");
            throw;
        }
    }

    private async Task<CompanyStatistics> GatherCompanyStatisticsAsync
        (Guid companyId, DateTime startDate, DateTime endDate)
    {
        return new CompanyStatistics
        {
            TotalReviews = await _context.Reviews
                .CountAsync(r =>
                    r.CompanyId == companyId.ToString() && r.CreatedAt >= startDate && r.CreatedAt <= endDate)
            , AverageRating = await _context.Reviews
                .Where(r => r.CompanyId == companyId.ToString() && r.CreatedAt >= startDate && r.CreatedAt <= endDate)
                .AverageAsync(r => (double?)r.OverallRating) ?? 0
            , TotalJobPostings = await _context.JobPostings
                .CountAsync(j =>
                    j.CompanyId == companyId.ToString() && j.CreatedAt >= startDate && j.CreatedAt <= endDate)
            , TotalApplications = await _context.CVApplications
                .CountAsync(a =>
                    a.Company != null && a.Company.Id == companyId.ToString() && a.CreatedAt >= startDate &&
                    a.CreatedAt <= endDate)
            , ActiveEmployees = await _context.Reviews
                .Where(r => r.CompanyId == companyId.ToString() && r.CommentType == CommentType.WorkEnvironment)
                .Select(r => r.UserId)
                .Distinct()
                .CountAsync()
        };
    }

    private string GenerateReportContent(Company company, CompanyStatistics stats, DateTime startDate, DateTime endDate)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"RateTheWork - Aylık Şirket Raporu");
        sb.AppendLine($"Şirket: {company.Name}");
        sb.AppendLine($"Dönem: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}");
        sb.AppendLine($"Oluşturulma Tarihi: {DateTime.UtcNow:dd.MM.yyyy HH:mm}");
        sb.AppendLine();
        sb.AppendLine("İSTATİSTİKLER");
        sb.AppendLine($"Toplam İnceleme: {stats.TotalReviews}");
        sb.AppendLine($"Ortalama Puan: {stats.AverageRating:F2}");
        sb.AppendLine($"Yayınlanan İş İlanı: {stats.TotalJobPostings}");
        sb.AppendLine($"Alınan Başvuru: {stats.TotalApplications}");
        sb.AppendLine($"Aktif Çalışan Sayısı: {stats.ActiveEmployees}");

        return sb.ToString();
    }

    private string GenerateSystemReportContent(SystemStatistics stats, DateTime startDate, DateTime endDate)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"RateTheWork - Haftalık Sistem Raporu");
        sb.AppendLine($"Dönem: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}");
        sb.AppendLine($"Oluşturulma Tarihi: {DateTime.UtcNow:dd.MM.yyyy HH:mm}");
        sb.AppendLine();
        sb.AppendLine("GENEL İSTATİSTİKLER");
        sb.AppendLine($"Toplam Kullanıcı: {stats.TotalUsers} (Yeni: {stats.NewUsers})");
        sb.AppendLine($"Toplam Şirket: {stats.TotalCompanies} (Yeni: {stats.NewCompanies})");
        sb.AppendLine($"Toplam İnceleme: {stats.TotalReviews} (Yeni: {stats.NewReviews})");
        sb.AppendLine($"Toplam İş İlanı: {stats.TotalJobPostings} (Yeni: {stats.NewJobPostings})");
        sb.AppendLine($"Toplam Başvuru: {stats.TotalApplications} (Yeni: {stats.NewApplications})");

        return sb.ToString();
    }

    private async Task SendReportEmailAsync(Company company, string fileUrl, DateTime reportDate)
    {
        // Şirket yöneticilerine email gönder (User entity'sinde CompanyId ve Role property'si yok, boş bırak)
        var admins = new List<User>(); // TODO: Company admins nasıl bulunacak belirlenmeli

        foreach (var admin in admins)
        {
            var subject = $"{company.Name} - {reportDate:MMMM yyyy} Raporu";
            var body = $@"
                <h2>Aylık Şirket Raporu</h2>
                <p>Sayın {admin.FullName},</p>
                <p>{company.Name} şirketinin {reportDate:MMMM yyyy} dönemi raporu hazırlanmıştır.</p>
                <p>Raporu indirmek için <a href='{fileUrl}'>buraya tıklayın</a>.</p>
                <br>
                <p>Saygılarımızla,<br>RateTheWork Ekibi</p>
            ";

            await _emailService.SendEmailAsync(admin.Email, subject, body, true);
        }
    }

    private class CompanyStatistics
    {
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public int TotalJobPostings { get; set; }
        public int TotalApplications { get; set; }
        public int ActiveEmployees { get; set; }
    }

    private class SystemStatistics
    {
        public int TotalUsers { get; set; }
        public int NewUsers { get; set; }
        public int TotalCompanies { get; set; }
        public int NewCompanies { get; set; }
        public int TotalReviews { get; set; }
        public int NewReviews { get; set; }
        public int TotalJobPostings { get; set; }
        public int NewJobPostings { get; set; }
        public int TotalApplications { get; set; }
        public int NewApplications { get; set; }
    }
}
