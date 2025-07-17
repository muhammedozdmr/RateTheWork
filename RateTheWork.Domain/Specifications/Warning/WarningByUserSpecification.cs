using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.Warning;

/// <summary>
/// Kullanıcıya göre uyarı spesifikasyonu
/// </summary>
public class WarningByUserSpecification : Specification<Entities.Warning>
{
    private readonly string _userId;

    public WarningByUserSpecification(string userId)
    {
        _userId = userId;
    }

    public override Expression<Func<Entities.Warning, bool>> ToExpression()
    {
        return warning => warning.UserId == _userId;
    }
}
