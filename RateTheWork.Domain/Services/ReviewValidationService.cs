using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.Interfaces.Services;

namespace RateTheWork.Domain.Services;

/// <summary>
/// Yorum validasyon işlemleri için service implementasyonu
/// </summary>
public class ReviewValidationService : IReviewValidationService
{
    private readonly IContentModerationService _contentModerationService;
    private readonly IReviewDomainService _reviewDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public ReviewValidationService
    (
        IUnitOfWork unitOfWork
        , IReviewDomainService reviewDomainService
        , IContentModerationService contentModerationService
    )
    {
        _unitOfWork = unitOfWork;
        _reviewDomainService = reviewDomainService;
        _contentModerationService = contentModerationService;
    }

    public async Task<bool> CanUserReviewCompanyAsync(string userId, string companyId, string commentType)
    {
        return await _reviewDomainService.CanUserReviewCompanyAsync(userId, companyId, commentType);
    }

    public async Task<bool> IsSpamReviewAsync(string userId, string commentText)
    {
        return await _reviewDomainService.IsSpamReviewAsync(userId, commentText);
    }

    public async Task<List<string?>> FindSimilarReviewsAsync(string userId, string commentText)
    {
        return await _reviewDomainService.FindSimilarReviewsAsync(userId, commentText);
    }

    public async Task<bool> ValidateReviewContentAsync(string commentText, string commentType)
    {
        // Boş içerik kontrolü
        if (string.IsNullOrWhiteSpace(commentText))
            return false;

        // Minimum uzunluk kontrolü
        if (commentText.Length < 50)
            return false;

        // Maksimum uzunluk kontrolü
        if (commentText.Length > 5000)
            return false;

        // İçerik moderasyonu
        var moderationResult = await _contentModerationService.ModerateReviewAsync(commentText, commentType);
        if (!moderationResult.IsApproved)
            return false;

        // Spam kontrolü
        if (await _contentModerationService.IsSpamPatternAsync(commentText))
            return false;

        return true;
    }
}
