using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.Review;

/// <summary>
/// Puan aralığına göre inceleme spesifikasyonu
/// </summary>
public class ReviewByRatingSpecification : Specification<Entities.Review>
{
    private readonly int _maxRating;
    private readonly int _minRating;

    public ReviewByRatingSpecification(int minRating, int maxRating)
    {
        _minRating = minRating;
        _maxRating = maxRating;
    }

    public override Expression<Func<Entities.Review, bool>> ToExpression()
    {
        return review => review.OverallRating >= _minRating && review.OverallRating <= _maxRating;
    }
}
