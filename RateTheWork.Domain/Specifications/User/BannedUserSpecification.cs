using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.User;

/// <summary>
/// Yasaklanmış kullanıcı spesifikasyonu
/// </summary>
public class BannedUserSpecification : Specification<Entities.User>
{
    public override Expression<Func<Entities.User, bool>> ToExpression()
    {
        return user => user.IsBanned;
    }
}
