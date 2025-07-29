using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork, Domain.Interfaces.Repositories.IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Dictionary<Type, object> _repositories;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        _repositories = new Dictionary<Type, object>();
    }

    public IUserRepository Users => GetRepository<IUserRepository, UserRepository>();
    public ICompanyRepository Companies => GetRepository<ICompanyRepository, CompanyRepository>();
    public IReviewRepository Reviews => GetRepository<IReviewRepository, ReviewRepository>();
    public IReviewVoteRepository ReviewVotes => GetRepository<IReviewVoteRepository, ReviewVoteRepository>();
    public IReportRepository Reports => GetRepository<IReportRepository, ReportRepository>();
    public IJobPostingRepository JobPostings => GetRepository<IJobPostingRepository, JobPostingRepository>();
    public IDepartmentRepository Departments => GetRepository<IDepartmentRepository, DepartmentRepository>();
    public IContractorReviewRepository ContractorReviews => GetRepository<IContractorReviewRepository, ContractorReviewRepository>();
    public ICVApplicationRepository CVApplications => GetRepository<ICVApplicationRepository, CVApplicationRepository>();
    public IJobApplicationRepository JobApplications => GetRepository<IJobApplicationRepository, JobApplicationRepository>();
    public IAuditLogRepository AuditLogs => GetRepository<IAuditLogRepository, AuditLogRepository>();

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private TInterface GetRepository<TInterface, TImplementation>() 
        where TInterface : class 
        where TImplementation : class, TInterface
    {
        var type = typeof(TInterface);

        if (!_repositories.ContainsKey(type))
        {
            var repositoryInstance = Activator.CreateInstance(typeof(TImplementation), _context);
            _repositories.Add(type, repositoryInstance!);
        }

        return (TInterface)_repositories[type];
    }
}