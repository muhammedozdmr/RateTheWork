using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces;

namespace RateTheWork.Domain.Services;

/// <summary>
/// Yorum işlemleri için domain service implementasyonu
/// </summary>
public class ReviewDomainService : IReviewDomainService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReviewDomainService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> CanUserReviewCompanyAsync(string userId, string companyId, string commentType)
    {
        // Her yorum türü için yılda bir kez yorum yapabilir
        var lastReview = await _unitOfWork.Reviews
            .GetFirstOrDefaultAsync(r => 
                r.UserId == userId && 
                r.CompanyId == companyId && 
                r.CommentType == commentType &&
                r.IsActive);

        if (lastReview == null)
            return true;

        var daysSinceLastReview = (DateTime.UtcNow - lastReview.CreatedAt).TotalDays;
        return daysSinceLastReview >= 365;
    }

    public decimal CalculateHelpfulnessScore(int upvotes, int downvotes, bool isVerified)
    {
        var totalVotes = upvotes + downvotes;
        if (totalVotes == 0)
            return 0;

        var baseScore = (decimal)upvotes / totalVotes * 100;
        
        // Doğrulanmış yorumlar için bonus
        if (isVerified)
            baseScore *= 1.2m;

        return Math.Round(baseScore, 2);
    }

    public async Task UpdateCompanyRatingAsync(string companyId)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null)
            throw new EntityNotFoundException($"Şirket bulunamadı: {companyId}");

        var reviews = await _unitOfWork.Reviews.GetReviewsByCompanyAsync(companyId, 1, int.MaxValue);
        var activeReviews = reviews.Where(r => r.IsActive).ToList();

        if (!activeReviews.Any())
        {
            company.UpdateReviewStatistics(0, 0);
            return;
        }

        var averageRating = activeReviews.Average(r => r.OverallRating);
        company.UpdateReviewStatistics(averageRating, activeReviews.Count);
    }

    public async Task<bool> IsSpamReviewAsync(string userId, string commentText)
    {
        // Son 24 saatte 3'ten fazla yorum yapmışsa spam olabilir
        var recentReviews = await _unitOfWork.Reviews
            .GetAsync(r => r.UserId == userId && r.CreatedAt > DateTime.UtcNow.AddDays(-1));

        if (recentReviews.Count > 3)
            return true;

        // Aynı metni içeren başka yorumlar var mı?
        var similarReviews = await _unitOfWork.Reviews
            .GetAsync(r => r.CommentText == commentText && r.UserId == userId);

        return similarReviews.Count > 0;
    }
}
