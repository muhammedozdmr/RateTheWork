using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.Company;

/// <summary>
/// Minimum yorum sayısına sahip şirket spesifikasyonu
/// </summary>
public class CompanyWithMinimumReviewsSpecification : Specification<Entities.Company>
{
    private readonly int _minimumReviews;

    public CompanyWithMinimumReviewsSpecification(int minimumReviews)
    {
        _minimumReviews = minimumReviews;
    }

    public override Expression<Func<Entities.Company, bool>> ToExpression()
    {
        return company => company.ReviewStatistics.TotalReviews >= _minimumReviews;
    }
}
