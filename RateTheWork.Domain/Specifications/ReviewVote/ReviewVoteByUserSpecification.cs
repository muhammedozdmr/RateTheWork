using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.ReviewVote;

/// <summary>
/// Kullanıcıya göre inceleme oyu spesifikasyonu
/// </summary>
public class ReviewVoteByUserSpecification : Specification<Entities.ReviewVote>
{
    private readonly string _userId;

    public ReviewVoteByUserSpecification(string userId)
    {
        _userId = userId;
    }

    public override Expression<Func<Entities.ReviewVote, bool>> ToExpression()
    {
        return vote => vote.UserId == _userId;
    }
}
