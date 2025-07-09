namespace RateTheWork.Domain.Interfaces.Specifications;

/// <summary>
/// Composite specification interface'i
/// Specification'ları birleştirmek için
/// </summary>
public interface ICompositeSpecification<T> : ISpecification<T>
{
    /// <summary>
    /// İki specification'ı AND ile birleştirir
    /// </summary>
    ISpecification<T> And(ISpecification<T> specification);
    
    /// <summary>
    /// İki specification'ı OR ile birleştirir
    /// </summary>
    ISpecification<T> Or(ISpecification<T> specification);
    
    /// <summary>
    /// Specification'ı NOT ile tersine çevirir
    /// </summary>
    ISpecification<T> Not();
}
