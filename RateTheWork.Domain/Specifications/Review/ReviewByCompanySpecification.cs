using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.Review;

/// <summary>
/// Şirkete göre inceleme spesifikasyonu
/// </summary>
public class ReviewByCompanySpecification : Specification<Entities.Review>
{
    private readonly string _companyId;

    public ReviewByCompanySpecification(string companyId)
    {
        _companyId = companyId;
    }

    public override Expression<Func<Entities.Review, bool>> ToExpression()
    {
        return review => review.CompanyId == _companyId;
    }
}
