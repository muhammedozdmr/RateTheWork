using Microsoft.EntityFrameworkCore.Storage;
using RateTheWork.Domain.Common;
using RateTheWork.Domain.Interfaces.Repositories;
using IUnitOfWork = RateTheWork.Application.Common.Interfaces.IUnitOfWork;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork, Domain.Interfaces.Repositories.IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();
    private ICompanyRepository? _companies;
    private ICompanyBranchRepository? _companyBranches;
    private IDbContextTransaction? _currentTransaction;
    private IJobApplicationRepository? _jobApplications;
    private IJobPostingRepository? _jobPostings;
    private IReviewRepository? _reviews;
    private IUserRepository? _users;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    // Application.Common.Interfaces.IUnitOfWork uygulaması
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public ICompanyRepository Companies => _companies ??= new CompanyRepository(_context);
    public IReviewRepository Reviews => _reviews ??= new ReviewRepository(_context);
    public IJobPostingRepository JobPostings => _jobPostings ??= new JobPostingRepository(_context);
    public IJobApplicationRepository JobApplications => _jobApplications ??= new JobApplicationRepository(_context);
    public ICompanyBranchRepository CompanyBranches => _companyBranches ??= new CompanyBranchRepository(_context);

    // TODO: Bu depoları uygula
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

    // CancellationToken ile Application.Common.Interfaces.IUnitOfWork metodları
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

    // CancellationToken olmadan Domain.Interfaces.Repositories.IUnitOfWork metodları
    public async Task BeginTransactionAsync()
    {
        await BeginTransactionAsync(CancellationToken.None);
    }

    public async Task CommitTransactionAsync()
    {
        await CommitTransactionAsync(CancellationToken.None);
    }

    public async Task RollbackTransactionAsync()
    {
        await RollbackTransactionAsync(CancellationToken.None);
    }

    // Domain.Interfaces.Repositories.IUnitOfWork uygulaması
    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        var type = typeof(T);
        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new GenericRepository<T>(_context);
        }

        return (IRepository<T>)_repositories[type];
    }

    public TRepository GetCustomRepository<TRepository>() where TRepository : class
    {
        var repositoryType = typeof(TRepository);

        return repositoryType.Name switch
        {
            nameof(IUserRepository) => (TRepository)(object)Users
            , nameof(ICompanyRepository) => (TRepository)(object)Companies
            , nameof(IReviewRepository) => (TRepository)(object)Reviews
            , nameof(IJobPostingRepository) => (TRepository)(object)JobPostings
            , nameof(IJobApplicationRepository) => (TRepository)(object)JobApplications
            , nameof(ICompanyBranchRepository) => (TRepository)(object)CompanyBranches
            , _ => throw new NotImplementedException($"Repository {repositoryType.Name} not implemented")
        };
    }

    public void Dispose()
    {
        _context.Dispose();
        _currentTransaction?.Dispose();
    }

    public async Task<int> CompleteAsync()
    {
        return await SaveChangesAsync();
    }
}
