using Microsoft.EntityFrameworkCore;
using RateTheWork.Domain.Entities;

namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// Entity Framework DbContext abstraction'ı.
/// Application katmanının Infrastructure'a bağımlı olmaması için kullanılır.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Kullanıcılar tablosu
    /// </summary>
    DbSet<User> Users { get; }
    
    /// <summary>
    /// Şirketler tablosu
    /// </summary>
    DbSet<Company> Companies { get; }
    
    /// <summary>
    /// Yorumlar tablosu
    /// </summary>
    DbSet<Review> Reviews { get; }
    
    /// <summary>
    /// Admin kullanıcıları tablosu
    /// </summary>
    DbSet<AdminUser> AdminUsers { get; }
    
    /// <summary>
    /// Doğrulama talepleri tablosu
    /// </summary>
    DbSet<VerificationRequest> VerificationRequests { get; }
    
    /// <summary>
    /// Yorum oyları tablosu
    /// </summary>
    DbSet<ReviewVote> ReviewVotes { get; }
    
    /// <summary>
    /// Rozetler tablosu
    /// </summary>
    DbSet<Badge> Badges { get; }
    
    /// <summary>
    /// Kullanıcı rozetleri tablosu
    /// </summary>
    DbSet<UserBadge> UserBadges { get; }
    
    /// <summary>
    /// Ban kayıtları tablosu
    /// </summary>
    DbSet<Ban> Bans { get; }
    
    /// <summary>
    /// Uyarı kayıtları tablosu
    /// </summary>
    DbSet<Warning> Warnings { get; }
    
    /// <summary>
    /// Şikayet kayıtları tablosu
    /// </summary>
    DbSet<Report> Reports { get; }
    
    /// <summary>
    /// Audit log kayıtları tablosu
    /// </summary>
    DbSet<AuditLog> AuditLogs { get; }
    
    /// <summary>
    /// Bildirimler tablosu
    /// </summary>
    DbSet<Notification> Notifications { get; }
    
    /// <summary>
    /// Değişiklikleri veritabanına kaydeder
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
