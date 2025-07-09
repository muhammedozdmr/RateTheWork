using System.Linq.Expressions;

namespace RateTheWork.Domain.Interfaces.Specifications;

/// <summary>
/// Specification pattern interface'i
/// İş kurallarını ve sorguları kapsüller
/// </summary>
/// <typeparam name="T">Entity tipi</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Specification'ın criteria expression'ı
    /// </summary>
    Expression<Func<T, bool>> Criteria { get; }
    
    /// <summary>
    /// Include edilecek navigation property'ler
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }
    
    /// <summary>
    /// Include edilecek string path'ler
    /// </summary>
    List<string> IncludeStrings { get; }
    
    /// <summary>
    /// Sıralama expression'ı
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }
    
    /// <summary>
    /// Ters sıralama expression'ı
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }
    
    /// <summary>
    /// Gruplama expression'ı
    /// </summary>
    Expression<Func<T, object>>? GroupBy { get; }
    
    /// <summary>
    /// Sayfalama aktif mi?
    /// </summary>
    bool IsPagingEnabled { get; }
    
    /// <summary>
    /// Atlanacak kayıt sayısı
    /// </summary>
    int Skip { get; }
    
    /// <summary>
    /// Alınacak kayıt sayısı
    /// </summary>
    int Take { get; }
    
    /// <summary>
    /// Entity'nin specification'ı karşılayıp karşılamadığını kontrol eder
    /// </summary>
    bool IsSatisfiedBy(T entity);
}
