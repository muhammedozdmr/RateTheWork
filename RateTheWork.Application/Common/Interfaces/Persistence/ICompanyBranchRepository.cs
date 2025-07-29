using RateTheWork.Domain.Entities;

namespace RateTheWork.Application.Common.Interfaces.Persistence;

/// <summary>
/// Company branch repository interface
/// </summary>
public interface ICompanyBranchRepository : IGenericRepository<CompanyBranch>
{
    Task<IReadOnlyList<CompanyBranch>> GetCompanyBranchesAsync(string companyId);
    Task<CompanyBranch?> GetHeadquartersAsync(string companyId);
    Task<int> GetBranchCountByCompanyAsync(string companyId);
}