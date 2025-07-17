using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.Ban;

/// <summary>
/// Kullanıcıya göre yasak spesifikasyonu
/// </summary>
public class BanByUserSpecification : Specification<Entities.Ban>
{
    private readonly string _userId;

    public BanByUserSpecification(string userId)
    {
        _userId = userId;
    }

    public override Expression<Func<Entities.Ban, bool>> ToExpression()
    {
        return ban => ban.UserId == _userId;
    }
}
