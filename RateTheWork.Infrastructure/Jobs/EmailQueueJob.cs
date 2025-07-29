using Hangfire;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;

namespace RateTheWork.Infrastructure.Jobs;

/// <summary>
/// Email kuyruğu işleme job'ı
/// </summary>
public class EmailQueueJob
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailQueueJob> _logger;
    private readonly IMetricsService _metricsService;

    public EmailQueueJob
    (
        IEmailService emailService
        , IMetricsService metricsService
        , ILogger<EmailQueueJob> logger
    )
    {
        _emailService = emailService;
        _metricsService = metricsService;
        _logger = logger;
    }

    /// <summary>
    /// Hoşgeldin emaili gönderir
    /// </summary>
    [Queue("critical")]
    public async Task SendWelcomeEmailAsync(Guid userId, string email, string fullName)
    {
        try
        {
            _logger.LogInformation("Hoşgeldin emaili gönderiliyor: {Email}", email);

            var subject = "RateTheWork'e Hoş Geldiniz!";
            var body = $@"
                <h2>Merhaba {fullName},</h2>
                <p>RateTheWork ailesine hoş geldiniz!</p>
                <p>Artık iş deneyimlerinizi paylaşabilir ve diğer çalışanların deneyimlerinden faydalanabilirsiniz.</p>
                <p>Hesabınızı doğrulamak için lütfen aşağıdaki linke tıklayın:</p>
                <a href='https://ratethework.com/verify/{userId}'>Hesabı Doğrula</a>
                <br><br>
                <p>Saygılarımızla,<br>RateTheWork Ekibi</p>
            ";

            var result = await _emailService.SendEmailAsync(email, subject, body, true);

            if (result.IsSuccess)
            {
                _metricsService.IncrementCounter("email_sent", new Dictionary<string, object?>
                {
                    { "type", "welcome" }
                });
                _logger.LogInformation("Hoşgeldin emaili başarıyla gönderildi: {Email}", email);
            }
            else
            {
                throw new Exception($"Email gönderilemedi: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hoşgeldin emaili gönderilemedi: {Email}", email);
            _metricsService.IncrementCounter("email_failed", new Dictionary<string, object?>
            {
                { "type", "welcome" }
            });
            throw;
        }
    }

    /// <summary>
    /// Şifre sıfırlama emaili gönderir
    /// </summary>
    [Queue("critical")]
    public async Task SendPasswordResetEmailAsync(string email, string resetToken)
    {
        try
        {
            _logger.LogInformation("Şifre sıfırlama emaili gönderiliyor: {Email}", email);

            var subject = "Şifre Sıfırlama Talebi";
            var body = $@"
                <h2>Şifre Sıfırlama</h2>
                <p>Şifrenizi sıfırlamak için aşağıdaki linke tıklayın:</p>
                <a href='https://ratethework.com/reset-password/{resetToken}'>Şifreyi Sıfırla</a>
                <p>Bu link 1 saat geçerlidir.</p>
                <p>Eğer bu talebi siz yapmadıysanız, bu emaili görmezden gelebilirsiniz.</p>
                <br>
                <p>Saygılarımızla,<br>RateTheWork Ekibi</p>
            ";

            var result = await _emailService.SendEmailAsync(email, subject, body, true);

            if (result.IsSuccess)
            {
                _metricsService.IncrementCounter("email_sent", new Dictionary<string, object?>
                {
                    { "type", "password_reset" }
                });
            }
            else
            {
                throw new Exception($"Email gönderilemedi: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şifre sıfırlama emaili gönderilemedi: {Email}", email);
            _metricsService.IncrementCounter("email_failed", new Dictionary<string, object?>
            {
                { "type", "password_reset" }
            });
            throw;
        }
    }

    /// <summary>
    /// İnceleme onay emaili gönderir
    /// </summary>
    [Queue("default")]
    public async Task SendReviewApprovedEmailAsync(string email, string companyName, Guid reviewId)
    {
        try
        {
            var subject = "İncelemeniz Onaylandı";
            var body = $@"
                <h2>İncelemeniz Onaylandı!</h2>
                <p>{companyName} hakkındaki incelemeniz onaylandı ve yayınlandı.</p>
                <p>İncelemenizi görüntülemek için <a href='https://ratethework.com/review/{reviewId}'>buraya tıklayın</a>.</p>
                <br>
                <p>Saygılarımızla,<br>RateTheWork Ekibi</p>
            ";

            var result = await _emailService.SendEmailAsync(email, subject, body, true);

            if (result.IsSuccess)
            {
                _metricsService.IncrementCounter("email_sent", new Dictionary<string, object?>
                {
                    { "type", "review_approved" }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İnceleme onay emaili gönderilemedi: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Başvuru durumu güncelleme emaili
    /// </summary>
    [Queue("default")]
    public async Task SendApplicationStatusEmailAsync
    (
        string email
        , string applicantName
        , string jobTitle
        , string companyName
        , string status
    )
    {
        try
        {
            var subject = $"Başvuru Durumu: {jobTitle}";
            var statusMessage = status switch
            {
                "Reviewed" => "incelendi", "Interview" => "mülakat aşamasına geçti"
                , "Rejected" => "maalesef olumsuz sonuçlandı", "Accepted" => "kabul edildi! Tebrikler!"
                , _ => "güncellendi"
            };

            var body = $@"
                <h2>Merhaba {applicantName},</h2>
                <p>{companyName} firmasındaki {jobTitle} pozisyonuna yaptığınız başvuru {statusMessage}.</p>
                <br>
                <p>Başvuru detaylarını hesabınızdan görüntüleyebilirsiniz.</p>
                <br>
                <p>Saygılarımızla,<br>RateTheWork Ekibi</p>
            ";

            var result = await _emailService.SendEmailAsync(email, subject, body, true);

            if (result.IsSuccess)
            {
                _metricsService.IncrementCounter("email_sent", new Dictionary<string, object?>
                {
                    { "type", "application_status" }, { "status", status }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Başvuru durumu emaili gönderilemedi: {Email}", email);
            throw;
        }
    }
}
