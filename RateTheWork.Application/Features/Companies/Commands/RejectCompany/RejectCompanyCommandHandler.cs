using FluentValidation;
using MediatR;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Enums;
using RateTheWork.Domain.Interfaces;

namespace RateTheWork.Application.Features.Companies.Commands.RejectCompany;

/// <summary>
/// Şirket reddetme komutu (Admin işlemi)
/// </summary>
public record RejectCompanyCommand : IRequest<RejectCompanyResult>
{
    /// <summary>
    /// Reddedilecek şirketin ID'si
    /// </summary>
    public string CompanyId { get; init; } = string.Empty;
    
    /// <summary>
    /// Red nedeni (kullanıcıya gösterilecek)
    /// </summary>
    public string RejectionReason { get; init; } = string.Empty;
    
    /// <summary>
    /// Kalıcı olarak mı reddediliyor? (true ise bir daha eklenemez)
    /// </summary>
    public bool IsPermanentRejection { get; init; }
}

/// <summary>
/// Şirket reddetme sonucu
/// </summary>
public record RejectCompanyResult
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// Şirket adı
    /// </summary>
    public string CompanyName { get; init; } = string.Empty;
    
    /// <summary>
    /// Şirketi ekleyen kullanıcıya bildirim gönderildi mi?
    /// </summary>
    public bool NotificationSent { get; init; }
    
    /// <summary>
    /// Sonuç mesajı
    /// </summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// RejectCompany command handler
/// </summary>
public class RejectCompanyCommandHandler : IRequestHandler<RejectCompanyCommand, RejectCompanyResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    public RejectCompanyCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _emailService = emailService;
    }

    public async Task<RejectCompanyResult> Handle(RejectCompanyCommand request, CancellationToken cancellationToken)
    {
        // 1. Admin kontrolü
        var isAdmin = _currentUserService.Roles.Contains(AdminRoles.SuperAdmin) || 
                     _currentUserService.Roles.Contains(AdminRoles.ContentManager) ||
                     _currentUserService.Roles.Contains(AdminRoles.Moderator);
        
        if (!isAdmin)
        {
            throw new ForbiddenAccessException("Bu işlem için admin yetkisi gerekmektedir.");
        }

        // 2. Şirketi getir
        var company = await _unitOfWork.Companies.GetByIdAsync(request.CompanyId);
        
        if (company == null)
        {
            throw new NotFoundException("Şirket bulunamadı.");
        }

        // 3. Zaten onaylı mı?
        if (company.IsApproved)
        {
            throw new BusinessRuleException("ALREADY_APPROVED", "Onaylanmış bir şirket reddedilemez. Önce silme işlemi yapınız.");
        }

        // 4. Silinmiş mi?
        if (company.IsDeleted)
        {
            return new RejectCompanyResult
            {
                Success = true,
                CompanyName = company.Name,
                NotificationSent = false,
                Message = "Bu şirket zaten silinmiş."
            };
        }

        // 5. Şirketi reddet
        company.IsApproved = false;
        company.ApprovedBy = _currentUserService.UserId;
        company.ApprovedAt = DateTime.UtcNow;
        company.ApprovalNotes = $"RED: {request.RejectionReason}";
        company.ModifiedAt = DateTime.UtcNow;
        company.ModifiedBy = _currentUserService.UserId;

        // 6. Kalıcı red ise sil
        if (request.IsPermanentRejection)
        {
            company.IsDeleted = true;
            company.DeletedAt = DateTime.UtcNow;
            company.DeletedBy = _currentUserService.UserId;
        }

        _unitOfWork.Companies.Update(company);

        // 7. Audit log oluştur
        var auditLog = new Domain.Entities.AuditLog(
            adminUserId: _currentUserService.UserId!,
            actionType: request.IsPermanentRejection ? "CompanyPermanentlyRejected" : "CompanyRejected",
            entityType: "Company",
            entityId: company.Id
        )
        {
            Details = $"Red nedeni: {request.RejectionReason}"
        };
        
        await _unitOfWork.AuditLogs.AddAsync(auditLog);

        // 8. Şirketi ekleyen kullanıcıya bildirim gönder
        var notificationSent = false;
        if (!string.IsNullOrEmpty(company.CreatedBy))
        {
            var user = await _unitOfWork.Users.GetByIdAsync(company.CreatedBy);
            if (user != null)
            {
                // Bildirim oluştur
                var notification = new Domain.Entities.Notification(
                    userId: user.Id,
                    type: "CompanyRejected",
                    message: $"Eklediğiniz '{company.Name}' şirketi reddedildi."
                )
                {
                    RelatedEntityId = company.Id,
                    RelatedEntityType = "Company"
                };
                
                await _unitOfWork.Notifications.AddAsync(notification);

                // Email gönder
                try
                {
                    var emailBody = request.IsPermanentRejection
                        ? $@"
                            <h2>Merhaba {user.AnonymousUsername},</h2>
                            <p>Eklediğiniz <strong>{company.Name}</strong> şirketi aşağıdaki nedenle reddedildi:</p>
                            <blockquote style='border-left: 3px solid #dc3545; padding-left: 10px; color: #666;'>
                                {request.RejectionReason}
                            </blockquote>
                            <p><strong>Bu şirket kalıcı olarak reddedilmiştir ve tekrar eklenemez.</strong></p>
                            <p>Başka sorularınız varsa destek ekibimizle iletişime geçebilirsiniz.</p>
                            <br>
                            <p>Saygılarımızla,<br>RateTheWork Ekibi</p>
                        "
                        : $@"
                            <h2>Merhaba {user.AnonymousUsername},</h2>
                            <p>Eklediğiniz <strong>{company.Name}</strong> şirketi aşağıdaki nedenle reddedildi:</p>
                            <blockquote style='border-left: 3px solid #ffc107; padding-left: 10px; color: #666;'>
                                {request.RejectionReason}
                            </blockquote>
                            <p>Belirtilen eksiklikleri giderdikten sonra şirketi tekrar ekleyebilirsiniz.</p>
                            <br>
                            <p>Saygılarımızla,<br>RateTheWork Ekibi</p>
                        ";

                    await _emailService.SendEmailAsync(
                        to: user.Email,
                        subject: "Şirket Ekleme Talebiniz Hakkında",
                        body: emailBody,
                        isHtml: true
                    );
                    notificationSent = true;
                }
                catch
                {
                    // Email gönderilemese bile işleme devam et
                    notificationSent = false;
                }
            }
        }

        // 9. Değişiklikleri kaydet
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var message = request.IsPermanentRejection
            ? $"'{company.Name}' şirketi kalıcı olarak reddedildi."
            : $"'{company.Name}' şirketi reddedildi. Kullanıcı düzeltme yapıp tekrar ekleyebilir.";

        return new RejectCompanyResult
        {
            Success = true,
            CompanyName = company.Name,
            NotificationSent = notificationSent,
            Message = message
        };
    }
}

/// <summary>
/// RejectCompany validation kuralları
/// </summary>
public class RejectCompanyCommandValidator : AbstractValidator<RejectCompanyCommand>
{
    public RejectCompanyCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("İşleminiz gerçekleştirilemedi. Lütfen sayfayı yenileyip tekrar deneyin.")
            .Must(BeAValidGuid).WithMessage("Geçersiz işlem. Lütfen şirket listesine dönüp tekrar deneyin.");

        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage("Red nedeni belirtmelisiniz.")
            .MinimumLength(20).WithMessage("Red nedeni en az 20 karakter olmalıdır. Kullanıcıya yeterli bilgi veriniz.")
            .MaximumLength(1000).WithMessage("Red nedeni 1000 karakteri aşamaz.");
    }

    private bool BeAValidGuid(string id)
    {
        return Guid.TryParse(id, out _);
    }
}
