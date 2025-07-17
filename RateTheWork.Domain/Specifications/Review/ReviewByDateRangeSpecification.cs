using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.Review;

/// <summary>
/// Tarih aralığına göre inceleme spesifikasyonu
/// </summary>
public class ReviewByDateRangeSpecification : Specification<Entities.Review>
{
    private readonly DateTime _endDate;
    private readonly DateTime _startDate;

    public ReviewByDateRangeSpecification(DateTime startDate, DateTime endDate)
    {
        _startDate = startDate;
        _endDate = endDate;
    }

    public override Expression<Func<Entities.Review, bool>> ToExpression()
    {
        return review => review.CreatedAt >= _startDate && review.CreatedAt <= _endDate;
    }
}
