using System.Linq.Expressions;

namespace RateTheWork.Domain.Specifications.Review;

public class ActiveReviewSpecification : Specification<Entities.Review>
{
    public override Expression<Func<Entities.Review, bool>> ToExpression()
    {
        return review => review.IsActive && review.ReportCount < 5;
    }
}
