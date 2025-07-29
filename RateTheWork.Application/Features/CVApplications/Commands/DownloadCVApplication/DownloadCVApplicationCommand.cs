using MediatR;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Interfaces;

namespace RateTheWork.Application.Features.CVApplications.Commands.DownloadCVApplication;

/// <summary>
/// CV başvurusunu indirme komutu
/// </summary>
public record DownloadCVApplicationCommand : IRequest<DownloadCVApplicationResult>
{
    /// <summary>
    /// CV başvuru ID'si
    /// </summary>
    public string ApplicationId { get; init; } = string.Empty;
}

/// <summary>
/// CV indirme sonucu
/// </summary>
public record DownloadCVApplicationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string CVFileUrl { get; init; } = string.Empty;
    public string? MotivationLetterUrl { get; init; }
    public DateTime DownloadedAt { get; init; }
    public DateTime FeedbackDeadline { get; init; }
    public bool IsFirstDownload { get; init; }
}

/// <summary>
/// CV indirme handler
/// </summary>
public class DownloadCVApplicationCommandHandler : IRequestHandler<DownloadCVApplicationCommand, DownloadCVApplicationResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DownloadCVApplicationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<DownloadCVApplicationResult> Handle(DownloadCVApplicationCommand request, CancellationToken cancellationToken)
    {
        // 1. CV başvurusunu getir
        var cvApplication = await _unitOfWork.CVApplications.GetByIdAsync(request.ApplicationId);
        if (cvApplication == null)
        {
            throw new NotFoundException("CV başvurusu bulunamadı.");
        }

        // 2. Yetki kontrolü
        var user = await _unitOfWork.Users.GetByIdAsync(_currentUserService.UserId!);
        if (user == null)
        {
            throw new UnauthorizedException("Bu işlem için yetkiniz yok.");
        }

        // Şirket yetkilisi mi kontrol et
        var hasCompanyRole = user.UserRoles.Any(role => 
            role == "CompanyAdmin" || 
            role == "CompanyHR" || 
            role == "CompanyManager");

        if (!hasCompanyRole)
        {
            throw new ForbiddenAccessException("CV'leri indirmek için şirket yetkilisi olmalısınız.");
        }

        // 3. Süresi dolmuş mu kontrol et
        if (cvApplication.IsExpired())
        {
            throw new BusinessRuleException("EXPIRED_APPLICATION", "Bu CV başvurusunun süresi dolmuş.");
        }

        // 4. İlk indirme mi kontrol et
        var isFirstDownload = !cvApplication.DownloadedAt.HasValue;

        // 5. İndirildi olarak işaretle
        cvApplication.MarkAsDownloaded(_currentUserService.UserId!);

        // 6. Değişiklikleri kaydet
        await _unitOfWork.CVApplications.UpdateAsync(cvApplication);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Kullanıcıya bildirim gönder
        if (isFirstDownload)
        {
            var notification = Notification.Create(
                userId: cvApplication.UserId,
                title: "CV'niz İndirildi",
                message: $"CV'niz {cvApplication.Company?.Name ?? "şirket"} tarafından indirildi. Şirketin size {cvApplication.FeedbackDeadline:dd.MM.yyyy} tarihine kadar geri dönüş yapması bekleniyor.",
                type: Domain.Enums.Notification.NotificationType.CVDownloaded,
                relatedEntityId: cvApplication.Id,
                relatedEntityType: "CVApplication"
            );

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // 8. Audit log oluştur
        var auditLog = Domain.Entities.AuditLog.Create(
            adminUserId: _currentUserService.UserId!,
            actionType: "CVDownloaded",
            entityType: "CVApplication",
            entityId: cvApplication.Id,
            details: $"CV indirildi. Geri bildirim son tarihi: {cvApplication.FeedbackDeadline:dd.MM.yyyy}",
            ipAddress: _currentUserService.IpAddress ?? "Unknown"
        );

        await _unitOfWork.AuditLogs.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DownloadCVApplicationResult
        {
            Success = true,
            Message = isFirstDownload 
                ? $"CV başarıyla indirildi. {cvApplication.FeedbackDeadline:dd.MM.yyyy} tarihine kadar başvurana geri dönüş yapmanız gerekmektedir." 
                : "CV daha önce indirilmişti.",
            CVFileUrl = cvApplication.CVFileUrl,
            MotivationLetterUrl = cvApplication.MotivationLetterUrl,
            DownloadedAt = cvApplication.DownloadedAt!.Value,
            FeedbackDeadline = cvApplication.FeedbackDeadline!.Value,
            IsFirstDownload = isFirstDownload
        };
    }
}