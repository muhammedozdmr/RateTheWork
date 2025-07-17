using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.Badge;

/// <summary>
/// Aktif rozet spesifikasyonu
/// </summary>
public class ActiveBadgeSpecification : Specification<Entities.Badge>
{
    public override Expression<Func<Entities.Badge, bool>> ToExpression()
    {
        return badge => badge.IsActive;
    }
}
