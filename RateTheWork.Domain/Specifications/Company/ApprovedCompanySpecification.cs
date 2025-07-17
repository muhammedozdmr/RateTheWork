using System.Linq.Expressions;
using RateTheWork.Domain.Enums.Company;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.Company;

public class ApprovedCompanySpecification : Specification<Entities.Company>
{
    public override Expression<Func<Entities.Company, bool>> ToExpression()
    {
        return company => company.IsApproved && company.ApprovalStatus == ApprovalStatus.Approved;
    }
}
