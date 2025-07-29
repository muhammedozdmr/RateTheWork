using FluentValidation;
using MediatR;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.CVApplication;
using RateTheWork.Domain.Interfaces;

namespace RateTheWork.Application.Features.CVApplications.Commands.RespondToCVApplication;

/// <summary>
/// CV başvurusuna yanıt verme komutu
/// </summary>
public record RespondToCVApplicationCommand : IRequest<RespondToCVApplicationResult>
{
    /// <summary>
    /// CV başvuru ID'si
    /// </summary>
    public string ApplicationId { get; init; } = string.Empty;
    
    /// <summary>
    /// Yanıt durumu (Accepted, Rejected, OnHold)
    /// </summary>
    public CVApplicationStatus Status { get; init; }
    
    /// <summary>
    /// Yanıt mesajı
    /// </summary>
    public string ResponseMessage { get; init; } = string.Empty;
    
    /// <summary>
    /// Email ile bildirim gönderilsin mi?
    /// </summary>
    public bool SendEmailNotification { get; init; } = true;
}

/// <summary>
/// CV başvuru yanıt sonucu
/// </summary>
public record RespondToCVApplicationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime RespondedAt { get; init; }
    public bool WithinDeadline { get; init; }
}

/// <summary>
/// CV başvuru yanıt handler
/// </summary>
public class RespondToCVApplicationCommandHandler : IRequestHandler<RespondToCVApplicationCommand, RespondToCVApplicationResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    public RespondToCVApplicationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _emailService = emailService;
    }

    public async Task<RespondToCVApplicationResult> Handle(RespondToCVApplicationCommand request, CancellationToken cancellationToken)
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

        var hasCompanyRole = user.UserRoles.Any(role => 
            role == "CompanyAdmin" || 
            role == "CompanyHR" || 
            role == "CompanyManager");

        if (!hasCompanyRole)
        {
            throw new ForbiddenAccessException("CV başvurularına yanıt vermek için şirket yetkilisi olmalısınız.");
        }

        // 3. Daha önce yanıt verilmiş mi kontrol et
        if (cvApplication.RespondedAt.HasValue)
        {
            throw new BusinessRuleException("ALREADY_RESPONDED", "Bu başvuruya zaten yanıt verilmiş.");
        }

        // 4. Geri bildirim süresi içinde mi kontrol et
        var withinDeadline = !cvApplication.IsFeedbackOverdue();

        // 5. Yanıt ver
        cvApplication.Respond(request.ResponseMessage, request.Status);

        // 6. Değişiklikleri kaydet
        await _unitOfWork.CVApplications.UpdateAsync(cvApplication);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Şirket puanını güncelle (geri bildirim süresi aşıldıysa puan düşür)
        if (!withinDeadline && cvApplication.DownloadedAt.HasValue)
        {
            var company = await _unitOfWork.Companies.GetByIdAsync(cvApplication.CompanyId);
            if (company != null)
            {
                // TODO: Şirket puanını düşürme mekanizması eklenecek
                // company.DecreaseFeedbackScore();
                await _unitOfWork.Companies.UpdateAsync(company);
            }
        }

        // 8. Kullanıcıya bildirim gönder
        var notificationTitle = request.Status switch
        {
            CVApplicationStatus.Accepted => "CV Başvurunuz Kabul Edildi!",
            CVApplicationStatus.Rejected => "CV Başvurunuz Değerlendirildi",
            CVApplicationStatus.OnHold => "CV Başvurunuz Beklemede",
            _ => "CV Başvurunuz Hakkında"
        };

        var notification = Notification.Create(
            userId: cvApplication.UserId,
            title: notificationTitle,
            message: $"{cvApplication.Company?.Name ?? "Şirket"} CV başvurunuza yanıt verdi. Detaylar için tıklayın.",
            type: Domain.Enums.Notification.NotificationType.CVResponded,
            relatedEntityId: cvApplication.Id,
            relatedEntityType: "CVApplication"
        );

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 9. Email bildirimi gönder
        if (request.SendEmailNotification)
        {
            var applicant = await _unitOfWork.Users.GetByIdAsync(cvApplication.UserId);
            if (applicant != null)
            {
                await _emailService.SendEmailAsync(
                    to: cvApplication.ApplicantEmail,
                    subject: notificationTitle,
                    body: $"Merhaba {cvApplication.ApplicantName},\n\n{request.ResponseMessage}",
                    cancellationToken: cancellationToken
                );
            }
        }

        // 10. Audit log
        var auditLog = Domain.Entities.AuditLog.Create(
            adminUserId: _currentUserService.UserId!,
            actionType: "CVApplicationResponded",
            entityType: "CVApplication",
            entityId: cvApplication.Id,
            details: $"CV başvurusuna yanıt verildi. Durum: {request.Status}, Süre içinde: {withinDeadline}",
            ipAddress: _currentUserService.IpAddress ?? "Unknown"
        );

        await _unitOfWork.AuditLogs.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RespondToCVApplicationResult
        {
            Success = true,
            Message = withinDeadline 
                ? "CV başvurusuna başarıyla yanıt verildi." 
                : "CV başvurusuna yanıt verildi ancak geri bildirim süresi aşılmıştı.",
            RespondedAt = cvApplication.RespondedAt!.Value,
            WithinDeadline = withinDeadline
        };
    }
}

/// <summary>
/// CV başvuru yanıt validasyon kuralları
/// </summary>
public class RespondToCVApplicationCommandValidator : AbstractValidator<RespondToCVApplicationCommand>
{
    public RespondToCVApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("CV başvuru ID'si belirtilmelidir.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Geçerli bir yanıt durumu seçilmelidir.")
            .Must(BeValidResponseStatus).WithMessage("Yanıt durumu Accepted, Rejected veya OnHold olmalıdır.");

        RuleFor(x => x.ResponseMessage)
            .NotEmpty().WithMessage("Yanıt mesajı girilmelidir.")
            .MinimumLength(20).WithMessage("Yanıt mesajı en az 20 karakter olmalıdır.")
            .MaximumLength(1000).WithMessage("Yanıt mesajı 1000 karakteri aşamaz.");
    }

    private bool BeValidResponseStatus(CVApplicationStatus status)
    {
        return status == CVApplicationStatus.Accepted ||
               status == CVApplicationStatus.Rejected ||
               status == CVApplicationStatus.OnHold;
    }
}