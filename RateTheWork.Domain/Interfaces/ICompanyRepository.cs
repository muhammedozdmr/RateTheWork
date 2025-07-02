using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces;

public interface ICompanyRepository : IBaseRepository<Company>
{
    Task<Company?> GetByTaxIdAsync(string taxId);
    Task<Company?> GetByMersisNoAsync(string mersisNo);
    Task<List<Company>> SearchCompaniesAsync(string searchTerm, string? sector = null, int page = 1, int pageSize = 20);
    Task<List<Company>> GetCompaniesBySectorAsync(string sector, int page = 1, int pageSize = 20);
    Task<List<Review>> GetCompanyReviewsAsync(string companyId, int page = 1, int pageSize = 10);
    Task<List<Company>> GetPendingApprovalCompaniesAsync();
    Task<bool> IsTaxIdTakenAsync(string taxId);
    Task<bool> IsMersisNoTakenAsync(string mersisNo);
    Task RecalculateAverageRatingAsync(string companyId);
    Task<decimal> CalculateWeightedAverageRatingAsync(string companyId);
}
