using System.Linq.Expressions;
using RateTheWork.Domain.Enums.VerificationRequest;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.VerificationRequest;

/// <summary>
/// Bekleyen doğrulama isteği spesifikasyonu
/// </summary>
public class PendingVerificationRequestSpecification : Specification<Entities.VerificationRequest>
{
    public override Expression<Func<Entities.VerificationRequest, bool>> ToExpression()
    {
        return request => request.Status == VerificationRequestStatus.Pending;
    }
}
