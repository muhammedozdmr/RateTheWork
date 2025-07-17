using System.Linq.Expressions;
using RateTheWork.Domain.Enums.Badge;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.Badge;

/// <summary>
/// Rozet türüne göre spesifikasyon
/// </summary>
public class BadgeByTypeSpecification : Specification<Entities.Badge>
{
    private readonly BadgeType _badgeType;

    public BadgeByTypeSpecification(BadgeType badgeType)
    {
        _badgeType = badgeType;
    }

    public override Expression<Func<Entities.Badge, bool>> ToExpression()
    {
        return badge => badge.Type == _badgeType;
    }
}
