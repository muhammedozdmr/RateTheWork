using FluentValidation;
using MediatR;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Interfaces;
using RateTheWork.Domain.Interfaces.Services;

namespace RateTheWork.Application.Features.ContractorReviews.Commands.CreateContractorReview;

/// <summary>
/// Contractor/Freelancer yorumu oluşturma komutu
/// </summary>
public record CreateContractorReviewCommand : IRequest<CreateContractorReviewResult>
{
    /// <summary>
    /// Yorum yapılacak şirket ID'si
    /// </summary>
    public string CompanyId { get; init; } = string.Empty;
    
    /// <summary>
    /// Proje açıklaması
    /// </summary>
    public string ProjectDescription { get; init; } = string.Empty;
    
    /// <summary>
    /// Proje bütçesi
    /// </summary>
    public decimal ProjectBudget { get; init; }
    
    /// <summary>
    /// Proje süresi
    /// </summary>
    public string ProjectDuration { get; init; } = string.Empty;
    
    /// <summary>
    /// Proje başlangıç tarihi
    /// </summary>
    public DateTime ProjectStartDate { get; init; }
    
    /// <summary>
    /// Proje bitiş tarihi
    /// </summary>
    public DateTime ProjectEndDate { get; init; }
    
    /// <summary>
    /// Ödeme zamanlaması puanı (1-5)
    /// </summary>
    public decimal PaymentTimelinessRating { get; init; }
    
    /// <summary>
    /// İletişim kalitesi puanı (1-5)
    /// </summary>
    public decimal CommunicationRating { get; init; }
    
    /// <summary>
    /// Proje yönetimi puanı (1-5)
    /// </summary>
    public decimal ProjectManagementRating { get; init; }
    
    /// <summary>
    /// Teknik yeterlilik puanı (1-5)
    /// </summary>
    public decimal TechnicalCompetenceRating { get; init; }
    
    /// <summary>
    /// Yorum metni
    /// </summary>
    public string ReviewText { get; init; } = string.Empty;
    
    /// <summary>
    /// Tekrar çalışır mıydınız?
    /// </summary>
    public bool WouldWorkAgain { get; init; }
    
    /// <summary>
    /// Anonim yorum mu?
    /// </summary>
    public bool IsAnonymous { get; init; } = true;
    
    /// <summary>
    /// Doğrulama belgesi URL'i (sözleşme/fatura)
    /// </summary>
    public string? VerificationDocumentUrl { get; init; }
}

/// <summary>
/// Contractor yorumu oluşturma sonucu
/// </summary>
public record CreateContractorReviewResult
{
    public string ReviewId { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public decimal OverallRating { get; init; }
    public bool IsVerified { get; init; }
}

/// <summary>
/// Contractor yorumu oluşturma handler
/// </summary>
public class CreateContractorReviewCommandHandler : IRequestHandler<CreateContractorReviewCommand, CreateContractorReviewResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IContentModerationService _moderationService;

    public CreateContractorReviewCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IContentModerationService moderationService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _moderationService = moderationService;
    }

    public async Task<CreateContractorReviewResult> Handle(CreateContractorReviewCommand request, CancellationToken cancellationToken)
    {
        // 1. Kullanıcı doğrulama
        if (string.IsNullOrEmpty(_currentUserService.UserId))
        {
            throw new ForbiddenAccessException("Bu işlem için giriş yapmalısınız.");
        }

        // 2. Şirket kontrolü
        var company = await _unitOfWork.Companies.GetByIdAsync(request.CompanyId);
        if (company == null)
        {
            throw new NotFoundException("Şirket bulunamadı.");
        }

        if (!company.IsActive)
        {
            throw new BusinessRuleException("INACTIVE_COMPANY", "Bu şirket için yorum yapılamaz.");
        }

        // 3. Daha önce yorum yapılmış mı kontrol et
        var hasExistingReview = await _unitOfWork.ContractorReviews.HasUserReviewedCompanyAsync(
            _currentUserService.UserId, 
            request.CompanyId);

        if (hasExistingReview)
        {
            throw new BusinessRuleException("EXISTING_REVIEW", "Bu şirket için zaten bir contractor yorumunuz bulunuyor.");
        }

        // 4. İçerik moderasyonu
        var moderationResult = await _moderationService.ModerateTextAsync(request.ReviewText);
        if (!moderationResult.IsApproved)
        {
            throw new BusinessRuleException("INAPPROPRIATE_CONTENT", 
                $"Yorumunuz uygun bulunmadı. Sebep: {moderationResult.RejectionReasons.FirstOrDefault()}");
        }

        // 5. Contractor yorumu oluştur
        var review = ContractorReview.Create(
            companyId: request.CompanyId,
            userId: _currentUserService.UserId,
            projectDescription: request.ProjectDescription,
            projectBudget: request.ProjectBudget,
            projectDuration: request.ProjectDuration,
            projectStartDate: request.ProjectStartDate,
            projectEndDate: request.ProjectEndDate,
            paymentTimelinessRating: request.PaymentTimelinessRating,
            communicationRating: request.CommunicationRating,
            projectManagementRating: request.ProjectManagementRating,
            technicalCompetenceRating: request.TechnicalCompetenceRating,
            reviewText: request.ReviewText,
            wouldWorkAgain: request.WouldWorkAgain,
            isAnonymous: request.IsAnonymous,
            verificationDocumentUrl: request.VerificationDocumentUrl
        );

        // 6. Veritabanına kaydet
        await _unitOfWork.ContractorReviews.AddAsync(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Şirket istatistiklerini güncelle
        var stats = await _unitOfWork.ContractorReviews.GetStatisticsAsync(request.CompanyId);
        
        // TODO: Şirket entity'sine contractor review istatistiklerini ekleme metodu eklenecek
        // company.UpdateContractorReviewStatistics(stats);
        // await _unitOfWork.Companies.UpdateAsync(company);

        // 8. Bildirim oluştur
        var notification = Notification.Create(
            userId: company.CreatedBy,
            title: "Yeni Contractor Yorumu",
            message: $"Şirketiniz hakkında yeni bir contractor/freelancer yorumu yapıldı.",
            type: Domain.Enums.Notification.NotificationType.NewReview,
            relatedEntityId: review.Id,
            relatedEntityType: "ContractorReview"
        );

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateContractorReviewResult
        {
            ReviewId = review.Id,
            Success = true,
            Message = review.IsVerified 
                ? "Contractor yorumunuz doğrulanmış olarak eklendi." 
                : "Contractor yorumunuz eklendi.",
            OverallRating = review.OverallRating,
            IsVerified = review.IsVerified
        };
    }
}

/// <summary>
/// Contractor yorumu validasyon kuralları
/// </summary>
public class CreateContractorReviewCommandValidator : AbstractValidator<CreateContractorReviewCommand>
{
    public CreateContractorReviewCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("Şirket seçilmelidir.");

        RuleFor(x => x.ProjectDescription)
            .NotEmpty().WithMessage("Proje açıklaması girilmelidir.")
            .MinimumLength(20).WithMessage("Proje açıklaması en az 20 karakter olmalıdır.")
            .MaximumLength(500).WithMessage("Proje açıklaması 500 karakteri aşamaz.");

        RuleFor(x => x.ProjectBudget)
            .GreaterThanOrEqualTo(0).WithMessage("Proje bütçesi negatif olamaz.");

        RuleFor(x => x.ProjectDuration)
            .NotEmpty().WithMessage("Proje süresi belirtilmelidir.")
            .Must(BeValidDuration).WithMessage("Geçerli bir proje süresi seçiniz.");

        RuleFor(x => x.ProjectEndDate)
            .GreaterThan(x => x.ProjectStartDate).WithMessage("Proje bitiş tarihi başlangıç tarihinden sonra olmalıdır.");

        RuleFor(x => x.PaymentTimelinessRating)
            .InclusiveBetween(1, 5).WithMessage("Ödeme zamanlaması puanı 1-5 arasında olmalıdır.");

        RuleFor(x => x.CommunicationRating)
            .InclusiveBetween(1, 5).WithMessage("İletişim puanı 1-5 arasında olmalıdır.");

        RuleFor(x => x.ProjectManagementRating)
            .InclusiveBetween(1, 5).WithMessage("Proje yönetimi puanı 1-5 arasında olmalıdır.");

        RuleFor(x => x.TechnicalCompetenceRating)
            .InclusiveBetween(1, 5).WithMessage("Teknik yeterlilik puanı 1-5 arasında olmalıdır.");

        RuleFor(x => x.ReviewText)
            .NotEmpty().WithMessage("Yorum metni girilmelidir.")
            .MinimumLength(50).WithMessage("Yorum en az 50 karakter olmalıdır.")
            .MaximumLength(2000).WithMessage("Yorum 2000 karakteri aşamaz.");

        When(x => !string.IsNullOrWhiteSpace(x.VerificationDocumentUrl), () =>
        {
            RuleFor(x => x.VerificationDocumentUrl)
                .Must(BeAValidUrl).WithMessage("Geçersiz doğrulama belgesi URL'i.");
        });
    }

    private bool BeValidDuration(string duration)
    {
        var validDurations = new[] 
        { 
            "1 aydan az", "1-3 ay", "3-6 ay", "6-12 ay", "1 yıldan fazla" 
        };
        return validDurations.Contains(duration);
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}