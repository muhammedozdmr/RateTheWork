using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    ICompanyRepository Companies { get; }
    IReviewRepository Reviews { get; }
    IAdminUserRepository AdminUsers { get; }
    IVerificationRequestRepository VerificationRequests { get; }
    IReviewVoteRepository ReviewVotes { get; }
    IBaseRepository<Badge> Badges { get; }
    IBaseRepository<UserBadge> UserBadges { get; }
    IBaseRepository<Ban> Bans { get; }
    IBaseRepository<Warning> Warnings { get; }
    IBaseRepository<Report> Reports { get; }
    IBaseRepository<AuditLog> AuditLogs { get; }
    IBaseRepository<Notification> Notifications { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
