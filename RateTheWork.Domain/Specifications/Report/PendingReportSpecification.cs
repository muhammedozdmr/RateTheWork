using System.Linq.Expressions;
using RateTheWork.Domain.Enums.Report;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.Report;

/// <summary>
/// Beklemede olan rapor spesifikasyonu
/// </summary>
public class PendingReportSpecification : Specification<Entities.Report>
{
    public override Expression<Func<Entities.Report, bool>> ToExpression()
    {
        return report => report.Status == ReportStatus.Pending;
    }
}
