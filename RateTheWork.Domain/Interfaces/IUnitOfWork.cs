using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern implementasyonu için interface.
/// Tüm repository'lere tek bir noktadan erişim sağlar ve transaction yönetimi yapar.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Kullanıcı repository'si
    /// </summary>
    IUserRepository Users { get; }
    
    /// <summary>
    /// Şirket repository'si
    /// </summary>
    ICompanyRepository Companies { get; }
    
    /// <summary>
    /// Yorum repository'si
    /// </summary>
    IReviewRepository Reviews { get; }
    
    /// <summary>
    /// Admin kullanıcı repository'si
    /// </summary>
    IAdminUserRepository AdminUsers { get; }
    
    /// <summary>
    /// Doğrulama talepleri repository'si
    /// </summary>
    IVerificationRequestRepository VerificationRequests { get; }
    
    /// <summary>
    /// Yorum oyları repository'si
    /// </summary>
    IReviewVoteRepository ReviewVotes { get; }
    
    /// <summary>
    /// Rozet repository'si
    /// </summary>
    IBaseRepository<Badge> Badges { get; }
    
    /// <summary>
    /// Kullanıcı rozetleri repository'si
    /// </summary>
    IBaseRepository<UserBadge> UserBadges { get; }
    
    /// <summary>
    /// Ban kayıtları repository'si
    /// </summary>
    IBaseRepository<Ban> Bans { get; }
    
    /// <summary>
    /// Uyarı kayıtları repository'si
    /// </summary>
    IBaseRepository<Warning> Warnings { get; }
    
    /// <summary>
    /// Şikayet kayıtları repository'si
    /// </summary>
    IBaseRepository<Report> Reports { get; }
    
    /// <summary>
    /// Audit log kayıtları repository'si
    /// </summary>
    IBaseRepository<AuditLog> AuditLogs { get; }
    
    /// <summary>
    /// Bildirim kayıtları repository'si
    /// </summary>
    IBaseRepository<Notification> Notifications { get; }
    
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