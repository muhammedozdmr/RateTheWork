using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.VerificationRequest;

/// <summary>
/// Kullanıcıya göre doğrulama isteği spesifikasyonu
/// </summary>
public class VerificationRequestByUserSpecification : Specification<Entities.VerificationRequest>
{
    private readonly string _userId;

    public VerificationRequestByUserSpecification(string userId)
    {
        _userId = userId;
    }

    public override Expression<Func<Entities.VerificationRequest, bool>> ToExpression()
    {
        return request => request.UserId == _userId;
    }
}
