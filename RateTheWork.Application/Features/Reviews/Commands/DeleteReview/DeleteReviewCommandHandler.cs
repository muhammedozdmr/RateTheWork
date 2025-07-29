using FluentValidation;
using MediatR;
using RateTheWork.Application.Common.Constants;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Enums;
using RateTheWork.Domain.Interfaces;

namespace RateTheWork.Application.Features.Reviews.Commands.DeleteReview;

/// <summary>
/// Yorum silme komutu
/// </summary>
public record DeleteReviewCommand : IRequest<DeleteReviewResult>
{
    /// <summary>
    /// Silinecek yorumun ID'si
    /// </summary>
    public string? ReviewId { get; init; } = string.Empty;
    
    /// <summary>
    /// Silme nedeni (admin siliyorsa zorunlu)
    /// </summary>
    public string? DeletionReason { get; init; }
}

/// <summary>
/// Yorum silme sonucu
/// </summary>
public record DeleteReviewResult
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// Kim tarafından silindi (Self, Admin)
    /// </summary>
    public string DeletedBy { get; init; } = string.Empty;
    
    /// <summary>
    /// Şirket puanı güncellendi mi?
    /// </summary>
    public bool CompanyRatingUpdated { get; init; }
    
    /// <summary>
    /// Sonuç mesajı
    /// </summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// DeleteReview command handler
/// </summary>
public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, DeleteReviewResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteReviewCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<DeleteReviewResult> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        // 1. Kullanıcı girişi kontrolü
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
        {
            throw new ForbiddenAccessException("Bu işlem için giriş yapmalısınız.");
        }

        // 2. Yorumu getir
        var review = await _unitOfWork.Reviews.GetByIdAsync(request.ReviewId);
        
        if (review == null)
        {
            throw new NotFoundException("Yorum bulunamadı.");
        }

        // 3. Zaten silinmiş mi?
        if (!review.IsActive)
        {
            return new DeleteReviewResult
            {
                Success = true,
                DeletedBy = "Already",
                CompanyRatingUpdated = false,
                Message = "Bu yorum zaten silinmiş."
            };
        }

        // 4. Yetki kontrolü
        var isAdmin = _currentUserService.Roles.Contains(AdminRoles.SuperAdmin) || 
                     _currentUserService.Roles.Contains(AdminRoles.Moderator);
        
        var isSelfDelete = review.UserId == _currentUserService.UserId;
        
        if (!isSelfDelete && !isAdmin)
        {
            throw new ForbiddenAccessException("Bu yorumu silme yetkiniz bulunmamaktadır.");
        }

        // 5. Admin siliyorsa neden zorunlu
        if (!isSelfDelete && string.IsNullOrWhiteSpace(request.DeletionReason))
        {
            throw new BusinessRuleException("DELETION_REASON_REQUIRED", 
                "Admin tarafından silinen yorumlar için silme nedeni zorunludur.");
        }

        // 6. 24 saat kuralı (sadece kullanıcı kendi yorumunu siliyorsa)
        if (isSelfDelete)
        {
            var hoursSinceCreation = (DateTime.UtcNow - review.CreatedAt).TotalHours;
            if (hoursSinceCreation > 24)
            {
                throw new BusinessRuleException("DELETE_TIME_EXPIRED", 
                    "Yorumlar sadece ilk 24 saat içinde silinebilir.");
            }
        }

        // 7. Yorumu soft delete yap - Domain method kullan
        review.Hide(_currentUserService.UserId!, request.DeletionReason ?? "Kullanıcı sildi");

        _unitOfWork.Reviews.Update(review);

        // 8. Admin siliyorsa audit log ve kullanıcı bildirimi
        if (!isSelfDelete)
        {
            // Audit log
            var auditLog = Domain.Entities.AuditLog.Create(
                adminUserId: _currentUserService.UserId!,
                actionType: "ReviewDeletedByAdmin",
                entityType: "Review",
                entityId: review.Id,
                details: $"Silme nedeni: {request.DeletionReason}"
            );
            
            await _unitOfWork.AuditLogs.AddAsync(auditLog);

            // Kullanıcıya bildirim
            var notification = Domain.Entities.Notification.Create(
                userId: review.UserId,
                type: Domain.Enums.Notification.NotificationType.ReviewRejected,
                title: "Yorumunuz Kaldırıldı",
                message: $"Yorumunuz yönetici tarafından kaldırıldı. Neden: {request.DeletionReason}",
                relatedEntityType: "Review",
                relatedEntityId: review.Id
            );
            
            await _unitOfWork.Notifications.AddAsync(notification);

            // Kullanıcıya uyarı ver (3 uyarıda ban)
            var user = await _unitOfWork.Users.GetByIdAsync(review.UserId);
            if (user != null)
            {
                var warning = Domain.Entities.Warning.Create(
                    userId: user.Id,
                    adminUserId: _currentUserService.UserId!,
                    type: Domain.Enums.User.WarningType.ContentViolation,
                    reason: $"Uygunsuz yorum: {request.DeletionReason}"
                );
                
                // Temporarily comment out warning storage
                // await _unitOfWork.Warnings.AddAsync(warning);
                
                user.IncrementWarningCount();
                
                // 3 uyarıda otomatik ban
                if (user.WarningCount >= 3 && !user.IsBanned)
                {
                    user.Ban();
                    
                    var ban = Domain.Entities.Ban.Create(
                        userId: user.Id,
                        adminUserId: _currentUserService.UserId!,
                        type: Domain.Enums.User.BanType.Temporary,
                        reason: "3 uyarı limiti aşıldı (otomatik ban)",
                        banDays: 30 // 30 gün ban
                    );

                    // Temporarily comment out ban storage
                    // await _unitOfWork.Bans.AddAsync(ban);
                }
                
                _unitOfWork.Users.Update(user);
            }
        }

        // 9. İlgili oyları sil
        var votes = await _unitOfWork.ReviewVotes.GetVotesForReviewAsync(review.Id);
        foreach (var vote in votes)
        {
            _unitOfWork.ReviewVotes.Delete(vote);
        }

        // 10. Şirket puan ortalamasını güncelle
        await _unitOfWork.Companies.RecalculateAverageRatingAsync(review.CompanyId);

        // 11. Değişiklikleri kaydet
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var deletedBy = isSelfDelete ? "Self" : "Admin";
        var message = isSelfDelete 
            ? "Yorumunuz başarıyla silindi." 
            : "Yorum yönetici tarafından kaldırıldı.";

        return new DeleteReviewResult
        {
            Success = true,
            DeletedBy = deletedBy,
            CompanyRatingUpdated = true,
            Message = message
        };
    }
}

/// <summary>
/// DeleteReview validation kuralları
/// </summary>
public class DeleteReviewCommandValidator : AbstractValidator<DeleteReviewCommand>
{
    public DeleteReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("İşleminiz gerçekleştirilemedi. Lütfen sayfayı yenileyip tekrar deneyin.")
            .Must(BeAValidGuid).WithMessage("Geçersiz işlem. Lütfen yorum listesine dönüp tekrar deneyin.");

        // Silme nedeni (admin için)
        When(x => !string.IsNullOrWhiteSpace(x.DeletionReason), () =>
        {
            RuleFor(x => x.DeletionReason!)
                .MinimumLength(10).WithMessage("Silme nedeni en az 10 karakter olmalıdır.")
                .MaximumLength(500).WithMessage("Silme nedeni 500 karakteri aşamaz.");
        });
    }

    private bool BeAValidGuid(string id)
    {
        return Guid.TryParse(id, out _);
    }
}
