using RateTheWork.Domain.Entities;

namespace RateTheWork.Application.Common.Interfaces.Persistence;

/// <summary>
/// Job posting repository interface
/// </summary>
public interface IJobPostingRepository : IGenericRepository<JobPosting>
{
    Task<IReadOnlyList<JobPosting>> GetActiveJobPostingsAsync(int pageNumber, int pageSize);
    Task<IReadOnlyList<JobPosting>> GetCompanyJobPostingsAsync(string companyId, bool activeOnly = true);
    Task<IReadOnlyList<JobPosting>> SearchJobPostingsAsync(string? keyword, string? location, 
        decimal? minSalary, decimal? maxSalary, bool? isRemote, int pageNumber, int pageSize);
    Task<int> GetActiveJobCountByCompanyAsync(string companyId);
}