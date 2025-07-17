using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.Report;

/// <summary>
/// Kullanıcıya göre rapor spesifikasyonu
/// </summary>
public class ReportByUserSpecification : Specification<Entities.Report>
{
    private readonly string _userId;

    public ReportByUserSpecification(string userId)
    {
        _userId = userId;
    }

    public override Expression<Func<Entities.Report, bool>> ToExpression()
    {
        return report => report.ReporterUserId == _userId;
    }
}
