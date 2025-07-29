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
            .Where(cr => cr.ReviewedById == reviewerId)
            .Include(cr => cr.Contractor)
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
            .Include(cr => cr.Contractor)
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
            .Include(cr => cr.Contractor)
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
            .Select(cr => cr.Rating)
            .ToListAsync();

        return Enumerable.Any(ratings) ? ratings.Average() : 0;
    }
}
