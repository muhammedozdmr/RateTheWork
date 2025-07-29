using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.Ban;

/// <summary>
/// Aktif yasak spesifikasyonu
/// </summary>
public class ActiveBanSpecification : Specification<Entities.Ban>
{
    public override Expression<Func<Entities.Ban, bool>> ToExpression()
    {
        return ban => ban.IsActive &&
                      (ban.ExpiresAt == null || ban.ExpiresAt > DateTime.UtcNow);
    }
}
