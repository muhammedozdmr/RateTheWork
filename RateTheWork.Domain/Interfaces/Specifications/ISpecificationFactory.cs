using System.Linq.Expressions;

namespace RateTheWork.Domain.Interfaces.Specifications;

/// <summary>
/// Specification factory interface'i
/// </summary>
public interface ISpecificationFactory
{
    /// <summary>
    /// Yeni specification oluşturur
    /// </summary>
    ISpecification<T> Create<T>(Expression<Func<T, bool>> criteria);
}
