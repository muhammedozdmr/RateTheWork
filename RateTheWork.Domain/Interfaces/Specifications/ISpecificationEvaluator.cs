namespace RateTheWork.Domain.Interfaces.Specifications;

/// <summary>
/// Specification evaluator interface'i
/// </summary>
public interface ISpecificationEvaluator<T> where T : class
{
    /// <summary>
    /// Query'e specification uygular
    /// </summary>
    IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification);
}
