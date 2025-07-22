namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern interface
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Değişiklikleri kaydeder
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Transaction başlatır
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Transaction'ı commit eder
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Transaction'ı rollback eder
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
