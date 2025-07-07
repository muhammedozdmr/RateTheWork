using FluentValidation;
using MediatR;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Interfaces;
using RateTheWork.Domain.Services;

namespace RateTheWork.Application.Features.Reviews.Commands.UpdateReview;

/// <summary>
/// Yorum güncelleme komutu
/// </summary>
public record UpdateReviewCommand : IRequest<UpdateReviewResult>
{
    /// <summary>
    /// Güncellenecek yorumun ID'si
    /// </summary>
    public string? ReviewId { get; init; } = string.Empty;
    
    /// <summary>
    /// Yeni puan (değiştirilmek istenmiyorsa null)
    /// </summary>
    public decimal? OverallRating { get; init; }
    
    /// <summary>
    /// Yeni yorum metni (değiştirilmek istenmiyorsa null)
    /// </summary>
    public string? CommentText { get; init; }
    
    /// <summary>
    /// Güncelleme nedeni (kullanıcıya gösterilmez, log için)
    /// </summary>
    public string? UpdateReason { get; init; }
}

/// <summary>
/// Yorum güncelleme sonucu
/// </summary>
public record UpdateReviewResult
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// Moderasyon durumu (güncelleme sonrası tekrar kontrol)
    /// </summary>
    public string ModerationStatus { get; init; } = string.Empty;
    
    /// <summary>
    /// Güncellenen alan sayısı
    /// </summary>
    public int UpdatedFieldsCount { get; init; }
    
    /// <summary>
    /// Sonuç mesajı
    /// </summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// UpdateReview command handler
/// </summary>
public class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand, UpdateReviewResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IContentModerationService _moderationService;
    private readonly IDateTimeService _dateTimeService;

    public UpdateReviewCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IContentModerationService moderationService,
        IDateTimeService dateTimeService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _moderationService = moderationService;
        _dateTimeService = dateTimeService;
    }

    public async Task<UpdateReviewResult> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        // 1. Kullanıcı girişi kontrolü
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
        {
            throw new ForbiddenAccessException("Bu işlem için giriş yapmalısınız.");
        }

        // 2. Yorum var mı?
        var review = await _unitOfWork.Reviews.GetByIdAsync(request.ReviewId);
        if (review == null)
        {
            throw new NotFoundException("Review", request.ReviewId);
        }

        // 3. Kullanıcı kendi yorumunu mu güncelliyor?
        if (review.UserId != _currentUserService.UserId)
        {
            throw new ForbiddenAccessException("Sadece kendi yorumunuzu güncelleyebilirsiniz.");
        }

        // 4. Yorum aktif mi? (silinmiş veya gizlenmiş olabilir)
        if (!review.IsActive)
        {
            throw new BusinessRuleException("REVIEW_INACTIVE", "Bu yorum güncellenemez.");
        }

        // 5. 24 saat kuralı - yorum 24 saatten eski ise güncellenemez
        var hoursSinceCreation = (_dateTimeService.UtcNow - review.CreatedAt).TotalHours;
        if (hoursSinceCreation > 24)
        {
            throw new BusinessRuleException("UPDATE_TIME_EXPIRED", 
                "Yorumlar sadece ilk 24 saat içinde güncellenebilir.");
        }

        // 6. Maksimum güncelleme sayısı kontrolü (3 kez)
        // TODO: Implement update count tracking
        // if (review.UpdateCount >= 3)
        // {
        //     throw new BusinessRuleException("MAX_UPDATES_REACHED", 
        //         "Bir yorum en fazla 3 kez güncellenebilir.");
        // }

        var updatedFieldsCount = 0;
        var requiresModeration = false;

        // 7. Puan güncelleme
        if (request.OverallRating.HasValue && request.OverallRating.Value != review.OverallRating)
        {
            review.OverallRating = request.OverallRating.Value;
            updatedFieldsCount++;
        }

        // 8. Yorum metni güncelleme
        if (!string.IsNullOrWhiteSpace(request.CommentText) && request.CommentText != review.CommentText)
        {
            // Yeni metin için moderasyon kontrolü
            var moderationResult = await _moderationService.ModerateContentAsync(request.CommentText);
            
            if (!moderationResult.IsApproved)
            {
                return new UpdateReviewResult
                {
                    Success = false,
                    ModerationStatus = "Rejected",
                    UpdatedFieldsCount = 0,
                    Message = $"Güncelleme reddedildi: {moderationResult.RejectionReason}"
                };
            }

            // Toxicity score yüksekse manuel incelemeye al
            if (moderationResult.ToxicityScore > 0.7)
            {
                requiresModeration = true;
            }

            review.CommentText = request.CommentText;
            updatedFieldsCount++;
        }

        // 9. Hiç değişiklik yoksa
        if (updatedFieldsCount == 0)
        {
            return new UpdateReviewResult
            {
                Success = true,
                ModerationStatus = "NoChange",
                UpdatedFieldsCount = 0,
                Message = "Güncellenecek bir değişiklik bulunamadı."
            };
        }

        // 10. Güncelleme bilgilerini set et
        review.ModifiedAt = _dateTimeService.UtcNow;
        review.ModifiedBy = _currentUserService.UserId;

        // 11. Manuel inceleme gerekiyorsa yorumu geçici olarak gizle
        if (requiresModeration)
        {
            review.IsActive = false;
        }

        // 12. Değişiklikleri kaydet
        _unitOfWork.Reviews.Update(review);
        
        // 13. Şirket puan ortalamasını güncelle (puan değiştiyse)
        if (request.OverallRating.HasValue)
        {
            await _unitOfWork.Companies.RecalculateAverageRatingAsync(review.CompanyId);
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 14. Güncelleme log'u oluştur
        // TODO: Create update log
        // await _unitOfWork.ReviewUpdateLogs.AddAsync(new ReviewUpdateLog { ... });

        var moderationStatus = requiresModeration ? "PendingReview" : "Approved";
        var message = requiresModeration 
            ? "Yorumunuz güncellendi ve incelemeye alındı." 
            : "Yorumunuz başarıyla güncellendi.";

        return new UpdateReviewResult
        {
            Success = true,
            ModerationStatus = moderationStatus,
            UpdatedFieldsCount = updatedFieldsCount,
            Message = message
        };
    }
}

/// <summary>
/// UpdateReview validation kuralları
/// </summary>
public class UpdateReviewCommandValidator : AbstractValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator()
    {
        // Review ID - Teknik alan, handler'da kontrol edilmeli
        // Yine de boş gelirse genel hata ver
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("İşleminiz gerçekleştirilemedi. Lütfen sayfayı yenileyip tekrar deneyin.")
            .Must(BeAValidGuid).WithMessage("Geçersiz işlem. Lütfen yorum listesine dönüp tekrar deneyin.");

        // En az bir alan güncellenmeli
        RuleFor(x => x)
            .Must(x => x.OverallRating.HasValue || !string.IsNullOrWhiteSpace(x.CommentText))
            .WithMessage("En az bir alan güncellenmelidir.");

        // Puan validation
        When(x => x.OverallRating.HasValue, () =>
        {
            RuleFor(x => x.OverallRating!.Value)
                .InclusiveBetween(1, 5).WithMessage("Puan 1-5 arasında olmalıdır.")
                .Must(BeValidRating).WithMessage("Puan 0.5'lik artışlarla verilebilir.");
        });

        // Yorum metni validation
        When(x => !string.IsNullOrWhiteSpace(x.CommentText), () =>
        {
            RuleFor(x => x.CommentText!)
                .MinimumLength(50).WithMessage("Yorum en az 50 karakter olmalıdır.")
                .MaximumLength(2000).WithMessage("Yorum 2000 karakterden uzun olamaz.")
                .Must(NotContainPersonalInfo).WithMessage("Yorum kişisel bilgi içeremez.");
        });

        // Güncelleme nedeni (opsiyonel)
        When(x => !string.IsNullOrWhiteSpace(x.UpdateReason), () =>
        {
            RuleFor(x => x.UpdateReason!)
                .MaximumLength(200).WithMessage("Güncelleme nedeni 200 karakterden uzun olamaz.");
        });
    }

    private bool BeAValidGuid(string id)
    {
        return Guid.TryParse(id, out _);
    }

    private bool BeValidRating(decimal rating)
    {
        return rating % 0.5m == 0;
    }

    private bool NotContainPersonalInfo(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return true;

        var patterns = new[]
        {
            @"\b\d{10,11}\b", // Telefon
            @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", // Email
            @"\b\d{11}\b" // TC Kimlik
        };

        foreach (var pattern in patterns)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(text, pattern))
                return false;
        }

        return true;
    }
}
