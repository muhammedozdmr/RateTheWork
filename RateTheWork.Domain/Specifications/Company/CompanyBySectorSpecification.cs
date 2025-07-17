using System.Linq.Expressions;
using RateTheWork.Domain.Enums.Company;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.Company;

/// <summary>
/// Sektöre göre şirket spesifikasyonu
/// </summary>
public class CompanyBySectorSpecification : Specification<Entities.Company>
{
    private readonly CompanySector _sector;

    public CompanyBySectorSpecification(CompanySector sector)
    {
        _sector = sector;
    }

    public override Expression<Func<Entities.Company, bool>> ToExpression()
    {
        return company => company.Sector == _sector;
    }
}
