namespace RateTheWork.Domain.Interfaces.Specifications;

/// <summary>
/// Asenkron specification interface'i
/// </summary>
public interface IAsyncSpecification<T> : ISpecification<T>
{
    /// <summary>
    /// Entity'nin specification'ı karşılayıp karşılamadığını asenkron kontrol eder
    /// </summary>
    Task<bool> IsSatisfiedByAsync(T entity, CancellationToken cancellationToken = default);
}
