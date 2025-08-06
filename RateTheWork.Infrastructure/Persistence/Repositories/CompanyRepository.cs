using Microsoft.EntityFrameworkCore;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.Company;
using RateTheWork.Domain.Enums.JobPosting;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

public class CompanyRepository : BaseRepository<Company>, ICompanyRepository
{
    public CompanyRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Company?> GetByTaxIdAsync(string taxId)
    {
        return await _dbSet
            .Include(c => c.Branches)
            .FirstOrDefaultAsync(c => c.TaxId == taxId);
    }

    public async Task<Company?> GetByMersisNoAsync(string mersisNo)
    {
        return await _dbSet
            .Include(c => c.Branches)
            .FirstOrDefaultAsync(c => c.MersisNo == mersisNo);
    }

    public async Task<List<Company>> SearchCompaniesAsync
        (string searchTerm, string? sector = null, int page = 1, int pageSize = 20)
    {
        var query = _dbSet
            .Where(c => c.Name.Contains(searchTerm) || c.Description.Contains(searchTerm));

        if (!string.IsNullOrEmpty(sector) && Enum.TryParse<CompanySector>(sector, true, out var sectorEnum))
        {
            query = query.Where(c => c.Sector == sectorEnum);
        }

        return await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Company>> GetCompaniesBySectorAsync(string sector, int page = 1, int pageSize = 20)
    {
        if (!Enum.TryParse<CompanySector>(sector, true, out var sectorEnum))
            return new List<Company>();

        return await _dbSet
            .Where(c => c.Sector == sectorEnum && c.IsActive)
            .Include(c => c.Branches)
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Review>> GetCompanyReviewsAsync(string companyId, int page = 1, int pageSize = 10)
    {
        if (!Guid.TryParse(companyId, out var companyGuid))
            return new List<Review>();

        return await _context.Set<Review>()
            .Where(r => r.CompanyId == companyId && r.IsActive && r.IsPublished)
            .Include(r => r.User)
            .Include(r => r.Branch)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Company>> GetPendingApprovalCompaniesAsync()
    {
        return await _dbSet
            .Where(c => !c.IsApproved && c.ApprovalStatus == ApprovalStatus.Pending)
            .Include(c => c.Branches)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> IsTaxIdTakenAsync(string taxId)
    {
        return await _dbSet.AnyAsync(c => c.TaxId == taxId);
    }

    public async Task<bool> IsMersisNoTakenAsync(string mersisNo)
    {
        return await _dbSet.AnyAsync(c => c.MersisNo == mersisNo);
    }

    public async Task RecalculateAverageRatingAsync(string? companyId)
    {
        if (string.IsNullOrEmpty(companyId) || !Guid.TryParse(companyId, out var companyGuid))
            return;

        var averageRating = await _context.Set<Review>()
            .Where(r => r.CompanyId == companyId && r.IsActive && r.IsPublished)
            .AverageAsync(r => (decimal?)r.OverallRating) ?? 0;

        var reviewCount = await _context.Set<Review>()
            .CountAsync(r => r.CompanyId == companyId && r.IsActive && r.IsPublished);

        var company = await _dbSet.FindAsync(companyId);
        if (company != null)
        {
            // Use the UpdateReviewStatistics method instead of direct property assignment
            company.UpdateReviewStatistics(averageRating, reviewCount);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<decimal> CalculateWeightedAverageRatingAsync(string companyId)
    {
        if (!Guid.TryParse(companyId, out var companyGuid))
            return 0;

        var reviews = await _context.Set<Review>()
            .Where(r => r.CompanyId == companyId && r.IsActive && r.IsPublished)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new { Rating = r.OverallRating, r.CreatedAt })
            .ToListAsync();

        if (!reviews.Any())
            return 0;

        // Calculate weighted average with more recent reviews having higher weight
        var now = DateTime.UtcNow;
        decimal weightedSum = 0;
        decimal totalWeight = 0;

        foreach (var review in reviews)
        {
            var daysSinceReview = (now - review.CreatedAt).TotalDays;
            var weight = Math.Max(1, 365 - daysSinceReview) / 365; // Weight decreases over time

            weightedSum += review.Rating * (decimal)weight;
            totalWeight += (decimal)weight;
        }

        return totalWeight > 0 ? weightedSum / totalWeight : 0;
    }

    public IQueryable<Company> GetQueryable()
    {
        return _dbSet;
    }

    async Task ICompanyRepository.UpdateAsync(Company company)
    {
        _dbSet.Update(company);
        await _context.SaveChangesAsync();
    }

    public void Update(Company company)
    {
        _dbSet.Update(company);
    }

    // Additional helper methods
    public async Task<Company?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        // Company doesn't have Slug property, find by normalized name instead
        var normalizedSlug = slug.ToLowerInvariant().Replace("-", " ");
        return await _dbSet
            .Include(c => c.Branches)
            .FirstOrDefaultAsync(c => c.Name.ToLower().Replace("-", " ").Contains(normalizedSlug), cancellationToken);
    }

    public async Task<Company?> GetByTaxNumberAsync(string taxNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.TaxId == taxNumber, cancellationToken);
    }

    public async Task<IEnumerable<Company>> GetActiveCompaniesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .Include(c => c.Branches)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Company>> GetCompaniesByOwnerAsync
        (Guid ownerId, CancellationToken cancellationToken = default)
    {
        // Since Company doesn't have OwnerId, we'll return empty list
        // This would need to be handled through a different relationship
        return new List<Company>();
    }

    public async Task<bool> IsSlugUniqueAsync
        (string slug, Guid? excludeCompanyId = null, CancellationToken cancellationToken = default)
    {
        // Since Company doesn't have Slug property, check by normalized name instead
        var normalizedSlug = slug.ToLowerInvariant().Replace("-", " ");
        var query = _dbSet.Where(c => c.Name.ToLower().Replace("-", " ").Contains(normalizedSlug));

        if (excludeCompanyId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCompanyId.Value.ToString());
        }

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<Company?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Branches)
            .FirstOrDefaultAsync(c => c.Id == id.ToString(), cancellationToken);
    }

    public async Task<int> GetActiveJobPostingCountAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<JobPosting>()
            .CountAsync(jp => jp.CompanyId == companyId.ToString() && jp.Status == JobPostingStatus.Active
                , cancellationToken);
    }

    // Blockchain metodları
    public async Task<List<Company>> GetBlockchainVerifiedCompaniesAsync(int page = 1, int pageSize = 20)
    {
        return await _dbSet
            .Where(c => c.IsVerifiedOnBlockchain)
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Company?> GetByBlockchainContractAddressAsync(string contractAddress)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.BlockchainContractAddress == contractAddress);
    }

    public async Task<bool> IsBlockchainContractAddressUniqueAsync(string contractAddress, Guid? excludeCompanyId = null)
    {
        var query = _dbSet.Where(c => c.BlockchainContractAddress == contractAddress);
        
        if (excludeCompanyId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCompanyId.Value.ToString());
        }

        return !await query.AnyAsync();
    }

    public async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Company, bool>> predicate)
    {
        return await _dbSet.CountAsync(predicate);
    }
}
