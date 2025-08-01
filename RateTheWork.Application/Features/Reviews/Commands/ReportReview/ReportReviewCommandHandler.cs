using FluentValidation;
using MediatR;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums;
using RateTheWork.Domain.Enums.Report;
using RateTheWork.Domain.Interfaces;
using RateTheWork.Domain.Enums.Notification;
using RateTheWork.Application.Common.Constants;

namespace RateTheWork.Application.Features.Reviews.Commands.ReportReview;

/// <summary>
/// Yorum şikayet komutu
/// </summary>
public record ReportReviewCommand : IRequest<ReportReviewResult>
{
    /// <summary>
    /// Şikayet edilecek yorumun ID'si
    /// </summary>
    public string? ReviewId { get; init; } = string.Empty;
    
    /// <summary>
    /// Şikayet nedeni
    /// </summary>
    public string ReportReason { get; init; } = string.Empty;
    
    /// <summary>
    /// Şikayet detayları (opsiyonel)
    /// </summary>
    public string? ReportDetails { get; init; }
}

/// <summary>
/// Yorum şikayet sonucu
/// </summary>
public record ReportReviewResult
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// Şikayet ID'si
    /// </summary>
    public string? ReportId { get; init; } = string.Empty;
    
    /// <summary>
    /// Otomatik aksiyon alındı mı?
    /// </summary>
    public bool AutoActionTaken { get; init; }
    
    /// <summary>
    /// Alınan otomatik aksiyon
    /// </summary>
    public string? AutoAction { get; init; }
    
    /// <summary>
    /// Sonuç mesajı
    /// </summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// ReportReview command handler
/// </summary>
public class ReportReviewCommandHandler : IRequestHandler<ReportReviewCommand, ReportReviewResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ReportReviewCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ReportReviewResult> Handle(ReportReviewCommand request, CancellationToken cancellationToken)
    {
        // 1. Kullanıcı girişi kontrolü
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
        {
            throw new ForbiddenAccessException("Şikayet için giriş yapmalısınız.");
        }

        // 2. Email doğrulama kontrolü
        var user = await _unitOfWork.Users.GetByIdAsync(_currentUserService.UserId);
        if (user == null || !user.IsEmailVerified)
        {
            throw new BusinessRuleException("EMAIL_NOT_VERIFIED", 
                "Şikayet gönderebilmek için email adresinizi doğrulamalısınız.");
        }

        // 3. Yorumu getir
        var review = await _unitOfWork.Reviews.GetByIdAsync(request.ReviewId);
        
        if (review == null || !review.IsActive)
        {
            throw new NotFoundException("Yorum bulunamadı veya silinmiş.");
        }

        // 4. Kendi yorumunu şikayet edemez
        if (review.UserId == _currentUserService.UserId)
        {
            throw new BusinessRuleException("SELF_REPORT", "Kendi yorumunuzu şikayet edemezsiniz.");
        }

        // 5. Daha önce şikayet etmiş mi?
        var existingReport = await _unitOfWork.Reports.GetFirstOrDefaultAsync(r => 
            r.ReportedEntityId == request.ReviewId && 
            r.ReporterUserId == _currentUserService.UserId &&
            r.Status == Domain.Enums.Report.ReportStatus.Pending);
        
        if (existingReport != null)
        {
            throw new BusinessRuleException("ALREADY_REPORTED", 
                "Bu yorumu zaten şikayet etmişsiniz. Şikayetiniz inceleniyor.");
        }

        // 6. Yeni şikayet oluştur
        var parsedReason = Enum.Parse<Domain.Enums.Report.ReportReason>(request.ReportReason);
        var report = Report.Create(
            reportedEntityType: "Review",
            reportedEntityId: request.ReviewId!,
            reporterUserId: _currentUserService.UserId!,
            reason: parsedReason,
            description: request.ReportDetails ?? string.Empty
        );

        await _unitOfWork.Reports.AddAsync(report);

        // 7. Yorumun şikayet sayısını artır
        review.IncrementReportCount();
        
        // 8. Otomatik aksiyon kontrolü
        var autoActionTaken = false;
        string? autoAction = null;
        
        // Domain modelindeki IncrementReportCount metodu otomatik gizleme yapar
        if (!review.IsActive)
        {
            autoActionTaken = true;
            autoAction = "ReviewAutoHidden";
            
            // Admin'lere bildirim
            await CreateAdminNotification(
                $"Yorum otomatik gizlendi (5+ şikayet). Yorum ID: {review.Id}",
                review.Id
            );
            
            // Yorum sahibine bildirim
            var reviewNotification = Notification.Create(
                userId: review.UserId,
                title: "Yorumunuz Gizlendi",
                message: "Yorumunuz çok fazla şikayet aldığı için otomatik olarak gizlenmiştir.",
                type: NotificationType.SystemAnnouncement,
                relatedEntityId: review.Id,
                relatedEntityType: "Review"
            );
            
            await _unitOfWork.Notifications.AddAsync(reviewNotification);
        }
        
        // Spam şikayetleri için özel kontrol
        if (parsedReason == Domain.Enums.Report.ReportReason.Spam)
        {
            var spamReportCount = (await _unitOfWork.Reports.GetAsync(r => 
                r.ReportedEntityId == request.ReviewId && 
                r.Reason == Domain.Enums.Report.ReportReason.Spam)).Count;
            
            // 3 spam şikayetinde otomatik incelemeye al
            if (spamReportCount >= 3)
            {
                // AI moderasyon için işaretle
                // TODO: Queue for AI moderation
                await CreateAdminNotification(
                    $"Yorum spam olarak işaretlendi (3+ spam şikayeti). AI moderasyona gönderildi.",
                    review.Id
                );
            }
        }

        _unitOfWork.Reviews.Update(review);

        // 9. Kullanıcının şikayet istatistiklerini kontrol et
        await CheckUserReportingPattern(_currentUserService.UserId, cancellationToken);

        // 10. Değişiklikleri kaydet
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var message = autoActionTaken 
            ? "Şikayetiniz alındı. Yorum çok sayıda şikayet aldığı için otomatik olarak gizlendi." 
            : "Şikayetiniz alındı ve en kısa sürede incelenecektir.";

        return new ReportReviewResult
        {
            Success = true,
            ReportId = report.Id,
            AutoActionTaken = autoActionTaken,
            AutoAction = autoAction,
            Message = message
        };
    }

    /// <summary>
    /// Admin'lere bildirim oluştur
    /// </summary>
    private async Task CreateAdminNotification(string message, string? reviewId)
    {
        // Tüm moderatör ve admin'leri bul
        var admins = await _unitOfWork.Users.GetAsync(u => 
            u.IsActive && 
            (u.Roles.Contains(AdminRoles.SuperAdmin) || u.Roles.Contains(AdminRoles.ContentManager)));

        foreach (var admin in admins)
        {
            var notification = Notification.Create(
                userId: admin.Id,
                title: "Yönetici Uyarısı",
                message: message,
                type: NotificationType.SystemAnnouncement,
                relatedEntityId: reviewId,
                relatedEntityType: "Review"
            );
            
            await _unitOfWork.Notifications.AddAsync(notification);
        }
    }

    /// <summary>
    /// Kullanıcının şikayet pattern'ini kontrol et (spam şikayet önleme)
    /// </summary>
    private async Task CheckUserReportingPattern(string userId, CancellationToken cancellationToken)
    {
        // Son 24 saatte yapılan şikayet sayısı
        var recentReportCount = (await _unitOfWork.Reports.GetAsync(r => 
            r.ReporterUserId == userId && 
            r.CreatedAt >= DateTime.UtcNow.AddHours(-24))).Count;
        
        // 10'dan fazla şikayet varsa uyarı
        if (recentReportCount > 10)
        {
            var auditLog = AuditLog.Create(
                adminUserId: "SYSTEM",
                actionType: "ExcessiveReporting",
                entityType: "User",
                entityId: userId,
                details: $"Kullanıcı 24 saatte {recentReportCount} şikayet gönderdi."
            );
            
            await _unitOfWork.AuditLogs.AddAsync(auditLog);
        }
    }
}

/// <summary>
/// ReportReview validation kuralları
/// </summary>
public class ReportReviewCommandValidator : AbstractValidator<ReportReviewCommand>
{
    public ReportReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("İşleminiz gerçekleştirilemedi. Lütfen sayfayı yenileyip tekrar deneyin.")
            .Must(BeAValidGuid).WithMessage("Geçersiz işlem. Lütfen yorum listesine dönüp tekrar deneyin.");

        RuleFor(x => x.ReportReason)
            .NotEmpty().WithMessage("Lütfen bir şikayet nedeni seçiniz.")
            .Must(BeValidReportReason).WithMessage("Geçersiz şikayet nedeni. Lütfen listeden seçiniz.");

        // Şikayet detayları (opsiyonel)
        When(x => !string.IsNullOrWhiteSpace(x.ReportDetails), () =>
        {
            RuleFor(x => x.ReportDetails!)
                .MaximumLength(500).WithMessage("Şikayet detayları 500 karakteri aşamaz.")
                .MinimumLength(10).WithMessage("Şikayet detayları en az 10 karakter olmalıdır.");
        });

        // "Diğer" seçiliyse detay zorunlu
        When(x => x.ReportReason == ReportReasons.Other.ToString(), () =>
        {
            RuleFor(x => x.ReportDetails)
                .NotEmpty().WithMessage("'Diğer' seçeneğini seçtiniz. Lütfen detay giriniz.")
                .MinimumLength(20).WithMessage("Şikayet detayları en az 20 karakter olmalıdır.");
        });
    }

    private bool BeAValidGuid(string id)
    {
        return Guid.TryParse(id, out _);
    }

    private bool BeValidReportReason(string reason)
    {
        return Enum.TryParse<Domain.Enums.Report.ReportReason>(reason, out _);
    }
}
