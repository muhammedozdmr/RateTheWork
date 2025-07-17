using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.ReviewVote;

/// <summary>
/// İncelemeye göre oy spesifikasyonu
/// </summary>
public class ReviewVoteByReviewSpecification : Specification<Entities.ReviewVote>
{
    private readonly string _reviewId;

    public ReviewVoteByReviewSpecification(string reviewId)
    {
        _reviewId = reviewId;
    }

    public override Expression<Func<Entities.ReviewVote, bool>> ToExpression()
    {
        return vote => vote.ReviewId == _reviewId;
    }
}
