using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.Review;

/// <summary>
/// Yayınlanmış inceleme spesifikasyonu
/// </summary>
public class PublishedReviewSpecification : Specification<Entities.Review>
{
    public override Expression<Func<Entities.Review, bool>> ToExpression()
    {
        return review => review.IsPublished && !review.IsDeleted;
    }
}
