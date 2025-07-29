using Microsoft.EntityFrameworkCore.Storage;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork, Domain.Interfaces.Repositories.IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _currentTransaction;
    private IUserRepository? _users;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    // Application.Common.Interfaces.IUnitOfWork implementation
    public IUserRepository Users => _users ??= new UserRepository(_context);
    
    // Stub implementations for now - TODO: Implement these repositories
    public ICompanyRepository Companies => throw new NotImplementedException();
    public IReviewRepository Reviews => throw new NotImplementedException();
    public IJobPostingRepository JobPostings => throw new NotImplementedException();
    public IJobApplicationRepository JobApplications => throw new NotImplementedException();
    public ICompanyBranchRepository CompanyBranches => throw new NotImplementedException();
    public IAuditLogRepository AuditLogs => throw new NotImplementedException();
    public INotificationRepository Notifications => throw new NotImplementedException();
    public IReviewVoteRepository ReviewVotes => throw new NotImplementedException();
    public IDepartmentRepository Departments => throw new NotImplementedException();
    public ICVApplicationRepository CVApplications => throw new NotImplementedException();
    public IContractorReviewRepository ContractorReviews => throw new NotImplementedException();
    public IReportRepository Reports => throw new NotImplementedException();
    public IReportRepository VerificationRequests => throw new NotImplementedException();

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    // Domain.Interfaces.Repositories.IUnitOfWork implementation
    public T Repository<T>() where T : class
    {
        throw new NotImplementedException();
    }

    public TRepository GetCustomRepository<TRepository>() where TRepository : class
    {
        throw new NotImplementedException();
    }

    public async Task<int> CompleteAsync()
    {
        return await SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        _currentTransaction?.Dispose();
    }
}