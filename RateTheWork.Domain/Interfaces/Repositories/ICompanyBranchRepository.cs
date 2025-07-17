using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces.Repositories;

/// <summary>
/// Şirket şubesi repository interface'i
/// </summary>
public interface ICompanyBranchRepository : IRepository<CompanyBranch>
{
    /// <summary>
    /// Şirketin tüm şubelerini getirir
    /// </summary>
    Task<IReadOnlyList<CompanyBranch>> GetBranchesByCompanyIdAsync(string companyId);

    /// <summary>
    /// Şirketin merkez ofisini getirir
    /// </summary>
    Task<CompanyBranch?> GetHeadquartersByCompanyIdAsync(string companyId);

    /// <summary>
    /// Belirli şehirdeki şubeleri getirir
    /// </summary>
    Task<IReadOnlyList<CompanyBranch>> GetBranchesByCityAsync(string city);

    /// <summary>
    /// Yakındaki şubeleri getirir
    /// </summary>
    Task<IReadOnlyList<CompanyBranch>> GetNearbyBranchesAsync(double latitude, double longitude, double radiusKm);

    /// <summary>
    /// Şirketin şube sayısını getirir
    /// </summary>
    Task<int> GetBranchCountByCompanyIdAsync(string companyId);

    /// <summary>
    /// Aktif şubeleri getirir
    /// </summary>
    Task<IReadOnlyList<CompanyBranch>> GetActiveBranchesByCompanyIdAsync(string companyId);
}
