using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.User;

/// <summary>
/// Doğrulanmış kullanıcı spesifikasyonu
/// </summary>
public class VerifiedUserSpecification : Specification<Entities.User>
{
    public override Expression<Func<Entities.User, bool>> ToExpression()
    {
        return user => user.IsVerified;
    }
}
