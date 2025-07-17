using System.Linq.Expressions;

namespace RateTheWork.Domain.Specifications.Common;

public class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> _specification;

    public NotSpecification(Specification<T> specification)
    {
        _specification = specification;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var expression = _specification.ToExpression();
        var parameter = Expression.Parameter(typeof(T));
        var body = Expression.Invoke(expression, parameter);

        return Expression.Lambda<Func<T, bool>>(
            Expression.Not(body), parameter);
    }
}
