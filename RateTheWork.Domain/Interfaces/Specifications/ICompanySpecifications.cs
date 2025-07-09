using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces.Specifications;

/// <summary>
/// Özel company specification'ları için interface
/// </summary>
public interface ICompanySpecifications
{
    ISpecification<Company> ActiveCompanies();
    ISpecification<Company> VerifiedCompanies();
    ISpecification<Company> CompaniesBySector(string sector);
    ISpecification<Company> CompaniesWithMinimumRating(decimal minRating);
    ISpecification<Company> BlacklistedCompanies();
    ISpecification<Company> CompaniesWithRecentReviews(int daysAgo);
}
