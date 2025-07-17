using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.AuditLog;

/// <summary>
/// Tarih aralığına göre denetim günlüğü spesifikasyonu
/// </summary>
public class AuditLogByDateRangeSpecification : Specification<Entities.AuditLog>
{
    private readonly DateTime _endDate;
    private readonly DateTime _startDate;

    public AuditLogByDateRangeSpecification(DateTime startDate, DateTime endDate)
    {
        _startDate = startDate;
        _endDate = endDate;
    }

    public override Expression<Func<Entities.AuditLog, bool>> ToExpression()
    {
        return log => log.PerformedAt >= _startDate && log.PerformedAt <= _endDate;
    }
}
