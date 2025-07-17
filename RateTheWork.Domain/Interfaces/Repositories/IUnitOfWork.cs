using RateTheWork.Domain.Common;
using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces.Repositories;

/// <summary>
/// Unit of Work pattern implementasyonu için interface.
/// Transaction yönetimi ve repository erişimi sağlar.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Convenience properties for common repositories
    /// <summary>
    /// Company repository
    /// </summary>
    IRepository<Company> Companies => Repository<Company>();

    /// <summary>
    /// Review repository
    /// </summary>
    IRepository<Review> Reviews => Repository<Review>();

    /// <summary>
    /// User repository
    /// </summary>
    IRepository<User> Users => Repository<User>();

    /// <summary>
    /// Generic repository döndürür
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <returns>İlgili entity için repository</returns>
    IRepository<T> Repository<T>() where T : BaseEntity;

    /// <summary>
    /// Özelleşmiş repository döndürür
    /// </summary>
    /// <typeparam name="TRepository">Repository interface tipi</typeparam>
    /// <returns>Özelleşmiş repository instance</returns>
    TRepository GetCustomRepository<TRepository>() where TRepository : class;

    /// <summary>
    /// Tüm değişiklikleri veritabanına kaydeder
    /// </summary>
    /// <param name="cancellationToken">İşlem iptali için token</param>
    /// <returns>Etkilenen kayıt sayısı</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Yeni bir transaction başlatır
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Aktif transaction'ı commit eder
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Aktif transaction'ı rollback eder
    /// </summary>
    Task RollbackTransactionAsync();
}
