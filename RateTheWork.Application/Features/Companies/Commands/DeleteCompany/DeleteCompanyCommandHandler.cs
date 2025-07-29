using FluentValidation;
using MediatR;
using RateTheWork.Application.Common.Constants;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Enums;
using RateTheWork.Domain.Interfaces;

namespace RateTheWork.Application.Features.Companies.Commands.DeleteCompany;

/// <summary>
/// Şirket silme komutu (Soft delete)
/// </summary>
public record DeleteCompanyCommand : IRequest<DeleteCompanyResult>
{
    /// <summary>
    /// Silinecek şirketin ID'si
    /// </summary>
    public string? CompanyId { get; init; } = string.Empty;
    
    /// <summary>
    /// Silme nedeni (admin log için)
    /// </summary>
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Şirket silme sonucu
/// </summary>
public record DeleteCompanyResult
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// Etkilenen kayıt sayıları
    /// </summary>
    public DeletedRecordsSummary DeletedRecords { get; init; } = new();
    
    /// <summary>
    /// Sonuç mesajı
    /// </summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Silinen kayıtların özeti
/// </summary>
public record DeletedRecordsSummary
{
    /// <summary>
    /// Gizlenen yorum sayısı
    /// </summary>
    public int HiddenReviewsCount { get; init; }
    
    /// <summary>
    /// İptal edilen doğrulama talebi sayısı
    /// </summary>
    public int CancelledVerificationRequestsCount { get; init; }
}

/// <summary>
/// DeleteCompany command handler
/// </summary>
public class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand, DeleteCompanyResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteCompanyCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<DeleteCompanyResult> Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
    {
        // 1. Admin kontrolü - Sadece admin silebilir
        var isAdmin = _currentUserService.Roles.Contains(AdminRoles.SuperAdmin) || 
                     _currentUserService.Roles.Contains(AdminRoles.ContentManager);
        
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

        // 3. Zaten silinmiş mi?
        if (company.IsDeleted)
        {
            return new DeleteCompanyResult
            {
                Success = true,
                DeletedRecords = new DeletedRecordsSummary(),
                Message = "Bu şirket zaten silinmiş."
            };
        }

        // 4. Transaction başlat
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 5. Şirketi soft delete yap
            company.SoftDelete(_currentUserService.UserId!);

            _unitOfWork.Companies.Update(company);

            // 6. Şirkete ait yorumları gizle (silme)
            var reviews = await _unitOfWork.Reviews.GetAsync(r => r.CompanyId == company.Id && r.IsActive);
            var hiddenReviewsCount = 0;

            foreach (var review in reviews)
            {
                review.Hide(_currentUserService.UserId, "Şirket silindiği için gizlendi");
                _unitOfWork.Reviews.Update(review);
                hiddenReviewsCount++;
            }

            // 7. Bekleyen şikayetleri iptal et 
            var pendingReports = await _unitOfWork.Reports.GetAsync(r => 
                r.Status == Domain.Enums.Report.ReportStatus.Pending &&
                reviews.Select(review => review.Id).Contains(r.ReportedEntityId));

            var cancelledRequestsCount = 0;
            foreach (var pendingReport in pendingReports)
            {
                pendingReport.Reject(_currentUserService.UserId!, "Şirket silindiği için otomatik iptal edildi.");
                await _unitOfWork.Reports.UpdateAsync(pendingReport);
                cancelledRequestsCount++;
            }

            // 8. Audit log oluştur
            var auditLog = Domain.Entities.AuditLog.Create(
                adminUserId: _currentUserService.UserId!,
                actionType: "Company.Deleted",
                entityType: "Company",
                entityId: company.Id,
                details: $"Silme nedeni: {request.Reason}. Gizlenen yorum: {hiddenReviewsCount}, İptal edilen doğrulama talebi: {cancelledRequestsCount}"
            );
            
            await _unitOfWork.AuditLogs.AddAsync(auditLog);

            // 9. Değişiklikleri kaydet ve transaction'ı commit et
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync();

            return new DeleteCompanyResult
            {
                Success = true,
                DeletedRecords = new DeletedRecordsSummary
                {
                    HiddenReviewsCount = hiddenReviewsCount,
                    CancelledVerificationRequestsCount = cancelledRequestsCount
                },
                Message = $"Şirket ve ilişkili {hiddenReviewsCount} yorum başarıyla silindi."
            };
        }
        catch (Exception)
        {
            // 10. Hata durumunda rollback
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}

/// <summary>
/// DeleteCompany validation kuralları
/// </summary>
public class DeleteCompanyCommandValidator : AbstractValidator<DeleteCompanyCommand>
{
    public DeleteCompanyCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("İşleminiz gerçekleştirilemedi. Lütfen sayfayı yenileyip tekrar deneyin.")
            .Must(BeAValidGuid).WithMessage("Geçersiz işlem. Lütfen şirket listesine dönüp tekrar deneyin.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Silme nedeni belirtmelisiniz.")
            .MinimumLength(10).WithMessage("Silme nedeni en az 10 karakter olmalıdır.")
            .MaximumLength(500).WithMessage("Silme nedeni 500 karakteri aşamaz.");
    }

    private bool BeAValidGuid(string id)
    {
        return Guid.TryParse(id, out _);
    }
}
