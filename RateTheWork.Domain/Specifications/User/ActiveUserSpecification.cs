using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.User;

public class ActiveUserSpecification : Specification<Entities.User>
{
    public override Expression<Func<Entities.User, bool>> ToExpression()
    {
        return user => !user.IsBanned && user.IsEmailVerified;
    }
}
