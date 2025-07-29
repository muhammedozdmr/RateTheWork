using FluentValidation;
using MediatR;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums;
using RateTheWork.Domain.Interfaces;
using RateTheWork.Domain.Interfaces.Services;
using RateTheWork.Domain.Services;

namespace RateTheWork.Application.Features.Reviews.Commands.CreateReview;
/// <summary>
/// Yeni yorum oluşturma komutu
/// </summary>
public record CreateReviewCommand : IRequest<CreateReviewResult>
{
    /// <summary>
    /// Yorum yapılacak şirket ID'si
    /// </summary>
    public string? CompanyId { get; init; } = string.Empty;
    
    /// <summary>
    /// Yorum türü (Maaş, Çalışma Ortamı vb.)
    /// </summary>
    public string CommentType { get; init; } = string.Empty;
    
    /// <summary>
    /// Genel puan (1-5 arası)
    /// </summary>
    public decimal OverallRating { get; init; }
    
    /// <summary>
    /// Yorum metni
    /// </summary>
    public string CommentText { get; init; } = string.Empty;
    
    /// <summary>
    /// Doğrulama belgesi URL'i (opsiyonel)
    /// </summary>
    public string? DocumentUrl { get; init; }
}

/// <summary>
/// Yorum oluşturma sonucu
/// </summary>
public record CreateReviewResult
{
    /// <summary>
    /// Oluşturulan yorumun ID'si
    /// </summary>
    public string? ReviewId { get; init; } = string.Empty;
    
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// Moderasyon durumu
    /// </summary>
    public string ModerationStatus { get; init; } = string.Empty; // Approved, Rejected, PendingReview
    
    /// <summary>
    /// Sonuç mesajı
    /// </summary>
    public string Message { get; init; } = string.Empty;
    
    /// <summary>
    /// Red nedeni (eğer reddedildiyse)
    /// </summary>
    public string? RejectionReason { get; init; }
}
/// <summary>
/// CreateReview command handler
/// </summary>
public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, CreateReviewResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IContentModerationService _moderationService;
    private readonly IDateTimeService _dateTimeService;

    public CreateReviewCommandHandler(
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

    public async Task<CreateReviewResult> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        // 1. Kullanıcı girişi kontrolü
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
        {
            throw new ForbiddenAccessException("Yorum yapmak için giriş yapmalısınız.");
        }

        // 2. Email doğrulama kontrolü
        var user = await _unitOfWork.Users.GetByIdAsync(_currentUserService.UserId);
        if (user == null || !user.IsEmailVerified)
        {
            throw new BusinessRuleException("EMAIL_NOT_VERIFIED", "Yorum yapmak için email adresinizi doğrulamalısınız.");
        }

        // 3. Kullanıcı banlı mı?
        if (user.IsBanned)
        {
            throw new ForbiddenAccessException("Hesabınız askıya alındığı için yorum yapamazsınız.");
        }

        // 4. Şirket var mı ve onaylı mı?
        var company = await _unitOfWork.Companies.GetByIdAsync(request.CompanyId);
        if (company == null || !company.IsApproved)
        {
            throw new NotFoundException("Company", request.CompanyId);
        }

        // 5. Kullanıcı bu şirkete aynı türde yorum yapmış mı?
        var hasReviewed = await _unitOfWork.Users.HasUserReviewedCompanyAsync(
            user.Id, company.Id, request.CommentType);
        
        if (hasReviewed)
        {
            throw new BusinessRuleException("ALREADY_REVIEWED", 
                $"Bu şirkete {request.CommentType} kategorisinde zaten yorum yapmışsınız.");
        }

        // 6. AI ile içerik moderasyonu
        var moderationResult = await _moderationService.ModerateContentAsync(request.CommentText);
        
        // 7. CommentType enum'a çevir
        if (!Enum.TryParse<Domain.Enums.Review.CommentType>(request.CommentType, out var commentType))
        {
            throw new BusinessRuleException("INVALID_COMMENT_TYPE", "Geçersiz yorum tipi.");
        }
        
        // 8. Yorum entity'si oluştur
        var review = Domain.Entities.Review.Create(
            companyId: company.Id,
            userId: user.Id,
            commentType: commentType,
            overallRating: request.OverallRating,
            commentText: request.CommentText,
            documentUrl: request.DocumentUrl
        );

        // 10. Moderasyon sonucuna göre işle
        if (!moderationResult.IsApproved)
        {
            // Yorum reddedildi ama yine de kaydet (admin panelinde görünsün)
            review.Deactivate();
            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Moderasyon log'u kaydet
            // TODO: Save moderation log
            
            return new CreateReviewResult
            {
                ReviewId = review.Id,
                Success = false,
                ModerationStatus = "Rejected",
                Message = "Yorumunuz içerik kurallarına aykırı bulundu.",
                RejectionReason = moderationResult.RejectionReasons?.FirstOrDefault()
            };
        }

        // 11. Toxicity score yüksekse manuel incelemeye al
        if (moderationResult.ToxicityScore > 0.7)
        {
            review.Deactivate(); // Manuel onay bekleyecek
            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateReviewResult
            {
                ReviewId = review.Id,
                Success = true,
                ModerationStatus = "PendingReview",
                Message = "Yorumunuz incelemeye alındı. Onaylandıktan sonra yayınlanacaktır."
            };
        }

        // 12. Yorum onaylandı - yayınla
        await _unitOfWork.Reviews.AddAsync(review);

        // 13. Şirketin puan ortalamasını güncelle
        await _unitOfWork.Companies.RecalculateAverageRatingAsync(company.Id);
        
        // 14. Değişiklikleri kaydet
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 15. İlk yorum rozeti kontrolü
        var userReviewCount = await _unitOfWork.Reviews.CountAsync(r => r.UserId == user.Id);
        if (userReviewCount == 1)
        {
            // TODO: Award first review badge
            // await _mediator.Send(new AwardBadgeCommand { UserId = user.Id, BadgeType = "FirstReview" });
        }

        return new CreateReviewResult
        {
            ReviewId = review.Id,
            Success = true,
            ModerationStatus = "Approved",
            Message = "Yorumunuz başarıyla yayınlandı!"
        };
    }
}

/// <summary>
/// CreateReview validation kuralları
/// </summary>
public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        // Şirket ID - Teknik alan ama kullanıcı dropdown'tan seçecek
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("Lütfen bir şirket seçiniz.")
            .Must(BeAValidGuid).WithMessage("Geçersiz şirket seçimi. Lütfen listeden bir şirket seçiniz.");

        // Yorum türü
        RuleFor(x => x.CommentType)
            .NotEmpty().WithMessage("Lütfen yorum kategorisini seçiniz.")
            .Must(BeValidCommentType).WithMessage("Geçersiz kategori. Lütfen listeden bir kategori seçiniz.");

        // Puan
        RuleFor(x => x.OverallRating)
            .InclusiveBetween(1, 5).WithMessage("Lütfen 1 ile 5 arasında bir puan veriniz.")
            .Must(BeValidRating).WithMessage("Puan tam sayı veya yarım puan (örn: 3.5) olmalıdır.");

        // Yorum metni
        RuleFor(x => x.CommentText)
            .NotEmpty().WithMessage("Lütfen yorumunuzu yazınız.")
            .MinimumLength(50).WithMessage("Yorumunuz en az 50 karakter olmalıdır. Daha detaylı bilgi veriniz.")
            .MaximumLength(2000).WithMessage("Yorumunuz 2000 karakteri aşamaz.")
            .Must(NotContainPersonalInfo).WithMessage("Yorumunuz telefon, email veya TC kimlik gibi kişisel bilgiler içeremez.");

        // Doküman URL
        When(x => !string.IsNullOrWhiteSpace(x.DocumentUrl), () =>
        {
            RuleFor(x => x.DocumentUrl)
                .Must(BeAValidUrl).WithMessage("Geçersiz belge bağlantısı. Lütfen geçerli bir URL giriniz.");
        });
    }

    private bool BeAValidGuid(string id)
    {
        return Guid.TryParse(id, out _);
    }

    private bool BeValidCommentType(string commentType)
    {
        return Enum.TryParse<Domain.Enums.Review.CommentType>(commentType, out _);
    }

    private bool BeValidRating(decimal rating)
    {
        // 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5 değerlerinden biri olmalı
        return rating % 0.5m == 0;
    }

    private bool NotContainPersonalInfo(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return true;

        // Basit kontroller - gerçek implementasyonda daha gelişmiş olmalı
        var patterns = new[]
        {
            @"\b\d{10,11}\b", // Telefon numarası
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

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
