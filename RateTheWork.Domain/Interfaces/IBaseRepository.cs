using System.Linq.Expressions;
using RateTheWork.Domain.Entities.Common;

namespace RateTheWork.Domain.Interfaces;

/// <summary>
/// Tüm repository'lerin temel interface'i.
/// Generic CRUD operasyonlarını tanımlar.
/// </summary>
/// <typeparam name="T">Entity tipi (BaseEntity'den türemeli)</typeparam>
public interface IBaseRepository<T> where T : BaseEntity
{
    /// <summary>
    /// ID'ye göre entity getirir
    /// </summary>
    /// <param name="id">Entity ID'si</param>
    /// <returns>Bulunan entity veya null</returns>
    Task<T?> GetByIdAsync(string id);

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
    /// Belirli koşula uyan entity sayısını getirir
    /// </summary>
    /// <param name="predicate">Filtreleme koşulu</param>
    /// <returns>Entity sayısı</returns>
    Task<int> GetCountAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Belirli koşula uyan en az bir entity var mı kontrol eder
    /// </summary>
    /// <param name="predicate">Filtreleme koşulu</param>
    /// <returns>Entity var mı?</returns>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

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

    /// <summary>
    /// Entity günceller
    /// </summary>
    /// <param name="entity">Güncellenecek entity</param>
    void Update(T entity);

    /// <summary>
    /// Birden fazla entity günceller
    /// </summary>
    /// <param name="entities">Güncellenecek entity listesi</param>
    void UpdateRange(IEnumerable<T> entities);

    /// <summary>
    /// Entity siler
    /// </summary>
    /// <param name="entity">Silinecek entity</param>
    void Delete(T entity);

    /// <summary>
    /// Birden fazla entity siler
    /// </summary>
    /// <param name="entities">Silinecek entity listesi</param>
    void DeleteRange(IEnumerable<T> entities);

    /// <summary>
    /// ID'ye göre entity siler
    /// </summary>
    /// <param name="id">Silinecek entity'nin ID'si</param>
    Task DeleteByIdAsync(string id);

    /// <summary>
    /// Sayfalı veri getirir
    /// </summary>
    /// <param name="predicate">Filtreleme koşulu</param>
    /// <param name="pageNumber">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <param name="orderBy">Sıralama fonksiyonu</param>
    /// <param name="ascending">Artan sıralama mı?</param>
    /// <returns>Sayfalı entity listesi</returns>
    Task<IReadOnlyList<T>> GetPagedAsync(
        Expression<Func<T, bool>>? predicate,
        int pageNumber,
        int pageSize,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true);

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
    /// IQueryable döner (advanced sorgular için)
    /// </summary>
    /// <returns>IQueryable<T></returns>
    IQueryable<T> GetQueryable();
}
