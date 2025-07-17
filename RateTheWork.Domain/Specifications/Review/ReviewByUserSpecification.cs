using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.Review;

/// <summary>
/// Kullanıcıya göre inceleme spesifikasyonu
/// </summary>
public class ReviewByUserSpecification : Specification<Entities.Review>
{
    private readonly string _userId;

    public ReviewByUserSpecification(string userId)
    {
        _userId = userId;
    }

    public override Expression<Func<Entities.Review, bool>> ToExpression()
    {
        return review => review.UserId == _userId;
    }
}
