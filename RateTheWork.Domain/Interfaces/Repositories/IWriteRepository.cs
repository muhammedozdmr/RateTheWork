using RateTheWork.Domain.Common;

namespace RateTheWork.Domain.Interfaces.Repositories;

/// <summary>
/// Yazma işlemleri için repository interface'i
/// </summary>
/// <typeparam name="T">Entity tipi (BaseEntity'den türemeli)</typeparam>
public interface IWriteRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Yeni entity ekler
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Birden fazla entity ekler
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Entity'yi günceller
    /// </summary>
    Task<T> UpdateAsync(T entity);

    /// <summary>
    /// Birden fazla entity'yi günceller
    /// </summary>
    Task UpdateRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Entity'yi siler
    /// </summary>
    Task DeleteAsync(T entity);

    /// <summary>
    /// ID'ye göre entity siler
    /// </summary>
    Task DeleteByIdAsync(string id);

    /// <summary>
    /// Birden fazla entity'yi siler
    /// </summary>
    Task DeleteRangeAsync(IEnumerable<T> entities);
}
