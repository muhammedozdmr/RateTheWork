using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.Company;

/// <summary>
/// Aktif şirket spesifikasyonu
/// </summary>
public class ActiveCompanySpecification : Specification<Entities.Company>
{
    public override Expression<Func<Entities.Company, bool>> ToExpression()
    {
        return company => company.IsActive && !company.IsDeleted;
    }
}
