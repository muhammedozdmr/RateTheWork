using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.User;

/// <summary>
/// E-posta adresine göre kullanıcı spesifikasyonu
/// </summary>
public class UserByEmailSpecification : Specification<Entities.User>
{
    private readonly string _email;

    public UserByEmailSpecification(string email)
    {
        _email = email;
    }

    public override Expression<Func<Entities.User, bool>> ToExpression()
    {
        return user => user.Email == _email;
    }
}
