using System.Linq.Expressions;

namespace RateTheWork.Domain.Specifications.Review;

public class VerifiedReviewSpecification : Specification<Entities.Review>
{
    public override Expression<Func<Entities.Review, bool>> ToExpression()
    {
        return review => review.IsDocumentVerified;
    }
}
