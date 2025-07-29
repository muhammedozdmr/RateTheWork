using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern interface
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// User repository
    /// </summary>
    IUserRepository Users { get; }
    
    /// <summary>
    /// Company repository
    /// </summary>
    ICompanyRepository Companies { get; }
    
    /// <summary>
    /// Review repository
    /// </summary>
    IReviewRepository Reviews { get; }
    
    /// <summary>
    /// Job posting repository
    /// </summary>
    IJobPostingRepository JobPostings { get; }
    
    /// <summary>
    /// Job application repository
    /// </summary>
    IJobApplicationRepository JobApplications { get; }
    
    /// <summary>
    /// Company branch repository
    /// </summary>
    ICompanyBranchRepository CompanyBranches { get; }
    
    /// <summary>
    /// Audit log repository
    /// </summary>
    IAuditLogRepository AuditLogs { get; }
    
    /// <summary>
    /// Notification repository
    /// </summary>
    INotificationRepository Notifications { get; }
    
    /// <summary>
    /// Review vote repository
    /// </summary>
    IReviewVoteRepository ReviewVotes { get; }
    
    /// <summary>
    /// Department repository
    /// </summary>
    IDepartmentRepository Departments { get; }
    
    /// <summary>
    /// CV Application repository
    /// </summary>
    ICVApplicationRepository CVApplications { get; }
    
    /// <summary>
    /// Contractor Review repository
    /// </summary>
    IContractorReviewRepository ContractorReviews { get; }
    
    /// <summary>
    /// Report repository
    /// </summary>
    IReportRepository Reports { get; }
    
    /// <summary>
    /// Verification Request repository
    /// </summary>
    IReportRepository VerificationRequests { get; } // Temp olarak IReportRepository kullanıyoruz
    
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
