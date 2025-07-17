using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.Warning;

/// <summary>
/// Aktif uyarı spesifikasyonu
/// </summary>
public class ActiveWarningSpecification : Specification<Entities.Warning>
{
    public override Expression<Func<Entities.Warning, bool>> ToExpression()
    {
        return warning => warning.IsActive &&
                          (warning.ExpiresAt == null || warning.ExpiresAt > DateTime.UtcNow);
    }
}
