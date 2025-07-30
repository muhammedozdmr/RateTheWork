using Microsoft.EntityFrameworkCore;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

/// <summary>
/// Müteahhit/taşeron firma incelemeleri için repository implementasyonu
/// </summary>
public class ContractorReviewRepository : BaseRepository<ContractorReview>, IContractorReviewRepository
{
    public ContractorReviewRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Şirkete ait contractor yorumlarını getirir
    /// </summary>
    public async Task<List<ContractorReview>> GetByCompanyIdAsync(string companyId, int page = 1, int pageSize = 20)
    {
        return await _context.ContractorReviews
            .Where(cr => cr.CompanyId == companyId)
            .Include(cr => cr.ReviewedBy)
            .OrderByDescending(cr => cr.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Kullanıcının contractor yorumlarını getirir
    /// </summary>
    public async Task<List<ContractorReview>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 20)
    {
        return await _context.ContractorReviews
            .Where(cr => cr.ReviewedById == userId)
            .Include(cr => cr.Company)
            .OrderByDescending(cr => cr.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Doğrulanmış contractor yorumlarını getirir
    /// </summary>
    public async Task<List<ContractorReview>> GetVerifiedByCompanyIdAsync
        (string companyId, int page = 1, int pageSize = 20)
    {
        return await _context.ContractorReviews
            .Where(cr => cr.CompanyId == companyId && cr.IsVerified)
            .Include(cr => cr.ReviewedBy)
            .OrderByDescending(cr => cr.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Tarih aralığına göre yorumları getirir
    /// </summary>
    public async Task<List<ContractorReview>> GetByDateRangeAsync
        (string companyId, DateTime startDate, DateTime endDate)
    {
        return await _context.ContractorReviews
            .Where(cr => cr.CompanyId == companyId &&
                         cr.CreatedAt >= startDate &&
                         cr.CreatedAt <= endDate)
            .Include(cr => cr.ReviewedBy)
            .OrderByDescending(cr => cr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// İstatistikleri getirir
    /// </summary>
    public async Task<ContractorReviewStatistics> GetStatisticsAsync(string companyId)
    {
        var reviews = await _context.ContractorReviews
            .Where(cr => cr.CompanyId == companyId)
            .ToListAsync();

        var statistics = new ContractorReviewStatistics
        {
            TotalReviews = reviews.Count, VerifiedReviews = reviews.Count(r => r.IsVerified)
            , AverageOverallRating = reviews.Any() ? (decimal)reviews.Average(r => r.OverallRating) : 0
            , AveragePaymentTimelinessRating =
                reviews.Any() ? (decimal)reviews.Average(r => r.PaymentTimelinessRating) : 0
            , AverageCommunicationRating = reviews.Any() ? (decimal)reviews.Average(r => r.CommunicationRating) : 0
            , AverageProjectManagementRating =
                reviews.Any() ? (decimal)reviews.Average(r => r.ProjectManagementRating) : 0
            , AverageTechnicalCompetenceRating =
                reviews.Any() ? (decimal)reviews.Average(r => r.TechnicalCompetenceRating) : 0
            , WouldWorkAgainCount = reviews.Count(r => r.WouldWorkAgain)
            , WouldWorkAgainPercentage =
                reviews.Any() ? (decimal)reviews.Count(r => r.WouldWorkAgain) * 100 / reviews.Count : 0
        };

        // Proje sürelerine göre dağılım
        statistics.ReviewsByDuration = reviews
            .GroupBy(r => r.ProjectDuration)
            .ToDictionary(g => g.Key, g => g.Count());

        // Aylık ortalama puanlar
        statistics.AverageRatingsByMonth = reviews
            .GroupBy(r => r.CreatedAt.ToString("yyyy-MM"))
            .ToDictionary(
                g => g.Key,
                g => g.Any() ? (decimal)g.Average(r => r.OverallRating) : 0
            );

        return statistics;
    }

    /// <summary>
    /// Kullanıcının şirkete daha önce contractor yorumu yapıp yapmadığını kontrol eder
    /// </summary>
    public async Task<bool> HasUserReviewedCompanyAsync(string userId, string companyId)
    {
        return await _context.ContractorReviews
            .AnyAsync(cr => cr.ReviewedById == userId && cr.CompanyId == companyId);
    }

    /// <summary>
    /// Müteahhit firmanın incelemelerini getirir
    /// </summary>
    public async Task<IEnumerable<ContractorReview>> GetByContractorIdAsync(Guid contractorId)
    {
        return await _context.ContractorReviews
            .Where(cr => cr.ContractorId == contractorId)
            .Include(cr => cr.ReviewedBy)
            .OrderByDescending(cr => cr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// İnceleyen kişinin yaptığı incelemeleri getirir
    /// </summary>
    public async Task<IEnumerable<ContractorReview>> GetByReviewerIdAsync(Guid reviewerId)
    {
        return await _context.ContractorReviews
            .Where(cr => cr.ReviewedById == reviewerId.ToString())
            .Include(cr => cr.Company)
            .OrderByDescending(cr => cr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Bekleyen incelemeleri getirir
    /// </summary>
    public async Task<IEnumerable<ContractorReview>> GetPendingReviewsAsync()
    {
        return await _context.ContractorReviews
            .Where(cr => cr.Status == "Pending")
            .Include(cr => cr.Company)
            .Include(cr => cr.ReviewedBy)
            .OrderBy(cr => cr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Belirli bir durumdaki incelemeleri getirir
    /// </summary>
    public async Task<IEnumerable<ContractorReview>> GetByStatusAsync(string status)
    {
        return await _context.ContractorReviews
            .Where(cr => cr.Status == status)
            .Include(cr => cr.Company)
            .Include(cr => cr.ReviewedBy)
            .OrderByDescending(cr => cr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Müteahhit için ortalama puanı hesaplar
    /// </summary>
    public async Task<double> GetAverageRatingAsync(Guid contractorId)
    {
        var ratings = await _context.ContractorReviews
            .Where(cr => cr.ContractorId == contractorId && cr.Status == "Approved")
            .Select(cr => cr.OverallRating)
            .ToListAsync();

        return ratings.Any() ? (double)ratings.Average() : 0;
    }
}
