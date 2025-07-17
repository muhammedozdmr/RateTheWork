using System.Linq.Expressions;
using RateTheWork.Domain.Common;

namespace RateTheWork.Domain.Interfaces.Repositories;

/// <summary>
/// Okuma işlemleri için repository interface'i
/// </summary>
/// <typeparam name="T">Entity tipi (BaseEntity'den türemeli)</typeparam>
public interface IReadRepository<T> where T : BaseEntity
{
    /// <summary>
    /// ID'ye göre entity getirir
    /// </summary>
    Task<T?> GetByIdAsync(string? id);

    /// <summary>
    /// Tüm entity'leri getirir
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync();

    /// <summary>
    /// Belirli koşula uyan entity'leri getirir
    /// </summary>
    Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Belirli koşula uyan ilk entity'yi getirir
    /// </summary>
    Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Include ile birlikte entity getirir
    /// </summary>
    Task<IReadOnlyList<T>> GetWithIncludesAsync
    (
        Expression<Func<T, bool>>? predicate
        , params Expression<Func<T, object>>[] includes
    );

    /// <summary>
    /// Sayfalı veri getirir
    /// </summary>
    Task<(IReadOnlyList<T> items, int totalCount)> GetPagedAsync
    (
        Expression<Func<T, bool>>? predicate = null
        , int pageNumber = 1
        , int pageSize = 10
        , Expression<Func<T, object>>? orderBy = null
        , bool ascending = true
    );

    /// <summary>
    /// Belirli koşula uyan entity sayısını getirir
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    /// <summary>
    /// Belirli koşula uyan en az bir entity var mı kontrol eder
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null);
}
