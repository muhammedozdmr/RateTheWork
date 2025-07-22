using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
    /// Üyelikler tablosu
    /// </summary>
    DbSet<Subscription> Subscriptions { get; }

    /// <summary>
    /// Şirket üyelikleri tablosu
    /// </summary>
    DbSet<CompanySubscription> CompanySubscriptions { get; }

    /// <summary>
    /// İş ilanları tablosu
    /// </summary>
    DbSet<JobPosting> JobPostings { get; }

    /// <summary>
    /// İK personeli tablosu
    /// </summary>
    DbSet<HRPersonnel> HRPersonnel { get; }

    /// <summary>
    /// İş başvuruları tablosu
    /// </summary>
    DbSet<JobApplication> JobApplications { get; }

    /// <summary>
    /// Şirket şubeleri tablosu
    /// </summary>
    DbSet<CompanyBranch> CompanyBranches { get; }

    /// <summary>
    /// Database facade for transaction management
    /// </summary>
    DatabaseFacade Database { get; }

    /// <summary>
    /// Check if there's an active transaction
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    /// Generic set method for accessing any entity
    /// </summary>
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    /// <summary>
    /// Değişiklikleri veritabanına kaydeder
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
