using FluentValidation;
using MediatR;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Enums;
using RateTheWork.Domain.Interfaces;

namespace RateTheWork.Application.Features.Companies.Commands.ApproveCompany;

/// <summary>
/// Şirket onaylama komutu (Admin işlemi)
/// </summary>
public record ApproveCompanyCommand : IRequest<ApproveCompanyResult>
{
    /// <summary>
    /// Onaylanacak şirketin ID'si
    /// </summary>
    public string? CompanyId { get; init; } = string.Empty;
    
    /// <summary>
    /// Onay notları (opsiyonel)
    /// </summary>
    public string? ApprovalNotes { get; init; }
}

/// <summary>
/// Şirket onaylama sonucu
/// </summary>
public record ApproveCompanyResult
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
/// ApproveCompany command handler
/// </summary>
public class ApproveCompanyCommandHandler : IRequestHandler<ApproveCompanyCommand, ApproveCompanyResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    public ApproveCompanyCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _emailService = emailService;
    }

    public async Task<ApproveCompanyResult> Handle(ApproveCompanyCommand request, CancellationToken cancellationToken)
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
            return new ApproveCompanyResult
            {
                Success = true,
                CompanyName = company.Name,
                NotificationSent = false,
                Message = "Bu şirket zaten onaylanmış."
            };
        }

        // 4. Silinmiş mi?
        if (company.IsDeleted)
        {
            throw new BusinessRuleException("DELETED_COMPANY", "Silinmiş bir şirket onaylanamaz.");
        }

        // 5. Şirketi onayla
        company.IsApproved = true;
        company.ApprovedBy = _currentUserService.UserId;
        company.ApprovedAt = DateTime.UtcNow;
        company.ApprovalNotes = request.ApprovalNotes;
        company.ModifiedAt = DateTime.UtcNow;
        company.ModifiedBy = _currentUserService.UserId;

        _unitOfWork.Companies.Update(company);

        // 6. Audit log oluştur
        var auditLog = new Domain.Entities.AuditLog(
            adminUserId: _currentUserService.UserId!,
            actionType: "CompanyApproved",
            entityType: "Company",
            entityId: company.Id
        )
        {
            Details = $"Şirket onaylandı: {company.Name}"
        };
        
        await _unitOfWork.AuditLogs.AddAsync(auditLog);

        // 7. Şirketi ekleyen kullanıcıya bildirim gönder
        var notificationSent = false;
        if (!string.IsNullOrEmpty(company.CreatedBy))
        {
            // Kullanıcıyı bul
            var user = await _unitOfWork.Users.GetByIdAsync(company.CreatedBy);
            if (user != null)
            {
                // Bildirim oluştur
                var notification = new Domain.Entities.Notification(
                    userId: user.Id,
                    type: "CompanyApproved",
                    message: $"Eklediğiniz '{company.Name}' şirketi onaylandı ve yayınlandı!"
                )
                {
                    RelatedEntityId = company.Id,
                    RelatedEntityType = "Company"
                };
                
                await _unitOfWork.Notifications.AddAsync(notification);

                // Email gönder
                try
                {
                    await _emailService.SendEmailAsync(
                        to: user.Email,
                        subject: "Şirketiniz Onaylandı!",
                        body: $@"
                            <h2>Merhaba {user.AnonymousUsername},</h2>
                            <p>Eklediğiniz <strong>{company.Name}</strong> şirketi yönetici tarafından onaylandı ve artık platformda yayınlanıyor.</p>
                            <p>Artık bu şirkete yorum yapabilir ve diğer kullanıcıların yorumlarını okuyabilirsiniz.</p>
                            <br>
                            <p>Saygılarımızla,<br>RateTheWork Ekibi</p>
                        ",
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

        // 8. Değişiklikleri kaydet
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApproveCompanyResult
        {
            Success = true,
            CompanyName = company.Name,
            NotificationSent = notificationSent,
            Message = $"'{company.Name}' şirketi başarıyla onaylandı ve yayınlandı."
        };
    }
}

/// <summary>
/// ApproveCompany validation kuralları
/// </summary>
public class ApproveCompanyCommandValidator : AbstractValidator<ApproveCompanyCommand>
{
    public ApproveCompanyCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("İşleminiz gerçekleştirilemedi. Lütfen sayfayı yenileyip tekrar deneyin.")
            .Must(BeAValidGuid).WithMessage("Geçersiz işlem. Lütfen şirket listesine dönüp tekrar deneyin.");

        // Onay notları opsiyonel
        When(x => !string.IsNullOrWhiteSpace(x.ApprovalNotes), () =>
        {
            RuleFor(x => x.ApprovalNotes!)
                .MaximumLength(500).WithMessage("Onay notları 500 karakteri aşamaz.");
        });
    }

    private bool BeAValidGuid(string id)
    {
        return Guid.TryParse(id, out _);
    }
}
