using Microsoft.EntityFrameworkCore;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

public class CompanyBranchRepository : BaseRepository<CompanyBranch>, ICompanyBranchRepository
{
    public CompanyBranchRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<CompanyBranch>> GetBranchesByCompanyIdAsync(string companyId)
    {
        return await _dbSet
            .Where(cb => cb.CompanyId == companyId)
            .OrderBy(cb => cb.Name)
            .ToListAsync();
    }

    public async Task<CompanyBranch?> GetHeadquartersByCompanyIdAsync(string companyId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(cb => cb.CompanyId == companyId && cb.IsHeadquarters);
    }

    public async Task<IReadOnlyList<CompanyBranch>> GetBranchesByCityAsync(string city)
    {
        return await _dbSet
            .Where(cb => cb.City == city)
            .Include(cb => cb.Company)
            .OrderBy(cb => cb.Company.Name)
            .ThenBy(cb => cb.Name)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<CompanyBranch>> GetNearbyBranchesAsync
        (double latitude, double longitude, double radiusKm)
    {
        // Calculate distance using Haversine formula
        // For simplicity, we'll use a rough approximation
        // 1 degree of latitude ≈ 111 km
        // 1 degree of longitude ≈ 111 km * cos(latitude)

        var latitudeDelta = radiusKm / 111.0;
        var longitudeDelta = radiusKm / (111.0 * Math.Cos(latitude * Math.PI / 180.0));

        var minLatitude = latitude - latitudeDelta;
        var maxLatitude = latitude + latitudeDelta;
        var minLongitude = longitude - longitudeDelta;
        var maxLongitude = longitude + longitudeDelta;

        var branches = await _dbSet
            .Where(cb => cb.Latitude >= minLatitude && cb.Latitude <= maxLatitude &&
                         cb.Longitude >= minLongitude && cb.Longitude <= maxLongitude)
            .Include(cb => cb.Company)
            .ToListAsync();

        // Calculate actual distances and filter
        var nearbyBranches = branches
            .Select(b => new
            {
                Branch = b, Distance = CalculateDistance(latitude, longitude, b.Latitude, b.Longitude)
            })
            .Where(x => x.Distance <= radiusKm)
            .OrderBy(x => x.Distance)
            .Select(x => x.Branch)
            .ToList();

        return nearbyBranches;
    }

    public async Task<int> GetBranchCountByCompanyIdAsync(string companyId)
    {
        if (!Guid.TryParse(companyId, out var companyGuid))
            return 0;

        return await _dbSet.CountAsync(cb => cb.CompanyId == companyId);
    }

    public async Task<IReadOnlyList<CompanyBranch>> GetActiveBranchesByCompanyIdAsync(string companyId)
    {
        if (!Guid.TryParse(companyId, out var companyGuid))
            return new List<CompanyBranch>();

        return await _dbSet
            .Where(cb => cb.CompanyId == companyId && cb.IsActive)
            .Include(cb => cb.JobPostings)
            .OrderBy(cb => cb.Name)
            .ToListAsync();
    }

    // Helper method to calculate distance between two points using Haversine formula
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in kilometers

        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLon = (lon2 - lon1) * Math.PI / 180.0;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    // Additional helper methods from original implementation
    public async Task<IEnumerable<CompanyBranch>> GetByCompanyAsync
        (Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cb => cb.CompanyId == companyId)
            .OrderBy(cb => cb.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<CompanyBranch?> GetHeadquartersAsync
        (Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(cb => cb.CompanyId == companyId && cb.IsHeadquarters, cancellationToken);
    }

    public async Task<IEnumerable<CompanyBranch>> GetByCityAsync
        (string city, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cb => cb.City == city)
            .Include(cb => cb.Company)
            .OrderBy(cb => cb.Company.Name)
            .ThenBy(cb => cb.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasActiveJobPostingsAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<JobPosting>()
            .AnyAsync(jp => jp.BranchId == branchId && jp.Status == JobPostingStatus.Active, cancellationToken);
    }

    public async Task<int> GetActiveJobPostingCountAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<JobPosting>()
            .CountAsync(jp => jp.BranchId == branchId && jp.Status == JobPostingStatus.Active, cancellationToken);
    }

    public async Task<CompanyBranch?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(cb => cb.Company)
            .Include(cb => cb.JobPostings)
            .ThenInclude(jp => jp.Applications)
            .Include(cb => cb.Reviews.Where(r => r.Status == ReviewStatus.Approved))
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(cb => cb.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync
        (Guid companyId, string name, Guid? excludeBranchId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(cb => cb.CompanyId == companyId && cb.Name == name);

        if (excludeBranchId.HasValue)
        {
            query = query.Where(cb => cb.Id != excludeBranchId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> GetCitiesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Select(cb => cb.City)
            .Distinct()
            .OrderBy(city => city)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> GetDistrictsByCityAsync
        (string city, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cb => cb.City == city)
            .Select(cb => cb.District)
            .Distinct()
            .OrderBy(district => district)
            .ToListAsync(cancellationToken);
    }
}
