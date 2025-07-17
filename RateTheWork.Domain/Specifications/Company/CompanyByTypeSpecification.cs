using System.Linq.Expressions;
using RateTheWork.Domain.Enums.Company;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.Company;

/// <summary>
/// Şirket türüne göre spesifikasyon
/// </summary>
public class CompanyByTypeSpecification : Specification<Entities.Company>
{
    private readonly CompanyType _companyType;

    public CompanyByTypeSpecification(CompanyType companyType)
    {
        _companyType = companyType;
    }

    public override Expression<Func<Entities.Company, bool>> ToExpression()
    {
        return company => company.CompanyType == _companyType;
    }
}
