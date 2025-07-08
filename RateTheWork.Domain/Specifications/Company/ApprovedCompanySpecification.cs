using System.Linq.Expressions;

namespace RateTheWork.Domain.Specifications.Company;

public class ApprovedCompanySpecification : Specification<Entities.Company>
{
    public override Expression<Func<Entities.Company, bool>> ToExpression()
    {
        return company => company.IsApproved && company.ApprovalStatus == "Approved";
    }
}
