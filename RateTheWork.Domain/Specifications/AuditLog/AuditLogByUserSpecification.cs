using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.AuditLog;

/// <summary>
/// Kullanıcıya göre denetim günlüğü spesifikasyonu
/// </summary>
public class AuditLogByUserSpecification : Specification<Entities.AuditLog>
{
    private readonly string _userId;

    public AuditLogByUserSpecification(string userId)
    {
        _userId = userId;
    }

    public override Expression<Func<Entities.AuditLog, bool>> ToExpression()
    {
        return log => log.AdminUserId == _userId;
    }
}
