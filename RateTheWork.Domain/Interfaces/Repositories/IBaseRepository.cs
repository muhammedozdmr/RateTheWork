using System.Linq.Expressions;
using RateTheWork.Domain.Common;

namespace RateTheWork.Domain.Interfaces.Repositories;

/// <summary>
/// Tüm repository'lerin temel interface'i.
/// Generic CRUD operasyonlarını tanımlar.
/// </summary>
/// <typeparam name="T">Entity tipi (BaseEntity'den türemeli)</typeparam>
public interface IBaseRepository<T> where T : BaseEntity
{
    // ============ READ OPERATIONS ============
    
    /// <summary>
    /// ID'ye göre entity getirir
    /// </summary>
    /// <param name="id">Entity ID'si</param>
    /// <returns>Bulunan entity veya null</returns>
    Task<T?> GetByIdAsync(string? id);

    /// <summary>
    /// Tüm entity'leri getirir
    /// </summary>
    /// <returns>Entity listesi</returns>
    Task<IReadOnlyList<T>> GetAllAsync();

    /// <summary>
    /// Belirli koşula uyan entity'leri getirir
    /// </summary>
    /// <param name="predicate">Filtreleme koşulu</param>
    /// <returns>Filtrelenmiş entity listesi</returns>
    Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Belirli koşula uyan ilk entity'yi getirir
    /// </summary>
    /// <param name="predicate">Filtreleme koşulu</param>
    /// <returns>Bulunan entity veya null</returns>
    Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Include ile birlikte entity getirir (navigation property'ler için)
    /// </summary>
    /// <param name="predicate">Filtreleme koşulu</param>
    /// <param name="includes">Include edilecek property'ler</param>
    /// <returns>Entity listesi</returns>
    Task<IReadOnlyList<T>> GetWithIncludesAsync(
        Expression<Func<T, bool>>? predicate,
        params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Sayfalı veri getirir
    /// </summary>
    /// <param name="predicate">Filtreleme koşulu (null ise tümü)</param>
    /// <param name="pageNumber">Sayfa numarası (1'den başlar)</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <param name="orderBy">Sıralama fonksiyonu (null ise Id'ye göre)</param>
    /// <param name="ascending">Artan sıralama mı? (default: true)</param>
    /// <returns>Sayfalı sonuç ve toplam kayıt sayısı</returns>
    Task<(IReadOnlyList<T> items, int totalCount)> GetPagedAsync(
        Expression<Func<T, bool>>? predicate = null,
        int pageNumber = 1,
        int pageSize = 10,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true);

    // ============ AGGREGATE OPERATIONS ============
    
    /// <summary>
    /// Belirli koşula uyan entity sayısını getirir
    /// </summary>
    /// <param name="predicate">Filtreleme koşulu</param>
    /// <returns>Entity sayısı</returns>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    /// <summary>
    /// Belirli koşula uyan en az bir entity var mı kontrol eder
    /// </summary>
    /// <param name="predicate">Filtreleme koşulu</param>
    /// <returns>Entity var mı?</returns>
    Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null);

    // ============ CREATE OPERATIONS ============
    
    /// <summary>
    /// Yeni entity ekler
    /// </summary>
    /// <param name="entity">Eklenecek entity</param>
    /// <returns>Eklenen entity</returns>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Birden fazla entity ekler
    /// </summary>
    /// <param name="entities">Eklenecek entity listesi</param>
    Task AddRangeAsync(IEnumerable<T> entities);

    // ============ UPDATE OPERATIONS ============
    
    /// <summary>
    /// Entity'yi günceller
    /// </summary>
    /// <param name="entity">Güncellenecek entity</param>
    /// <returns>Güncellenen entity</returns>
    Task<T> UpdateAsync(T entity);
    
    /// <summary>
    /// Birden fazla entity'yi günceller
    /// </summary>
    /// <param name="entities">Güncellenecek entity listesi</param>
    Task UpdateRangeAsync(IEnumerable<T> entities);

    // ============ DELETE OPERATIONS ============
    
    /// <summary>
    /// Entity'yi siler
    /// </summary>
    /// <param name="entity">Silinecek entity</param>
    Task DeleteAsync(T entity);
    
    /// <summary>
    /// ID'ye göre entity siler
    /// </summary>
    /// <param name="id">Silinecek entity'nin ID'si</param>
    Task DeleteByIdAsync(string id);
    
    /// <summary>
    /// Birden fazla entity'yi siler
    /// </summary>
    /// <param name="entities">Silinecek entity listesi</param>
    Task DeleteRangeAsync(IEnumerable<T> entities);
}