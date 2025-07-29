using RateTheWork.Domain.Entities;

namespace RateTheWork.Application.Common.Interfaces.Persistence;

/// <summary>
/// Company repository interface
/// </summary>
public interface ICompanyRepository : IGenericRepository<Company>
{
    Task<Company?> GetByNameAsync(string name);
    Task<bool> IsNameUniqueAsync(string name, string? excludeCompanyId = null);
    Task<IReadOnlyList<Company>> GetApprovedCompaniesAsync();
    Task<IReadOnlyList<Company>> GetPendingCompaniesAsync();
    Task<IReadOnlyList<Company>> SearchCompaniesAsync(string searchTerm, int pageNumber, int pageSize);
    Task RecalculateAverageRatingAsync(string companyId);
    Task<int> GetEmployeeCountBySectorAsync(string sector);
}