using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Application.Common.Models;
using RateTheWork.Application.Services.Interfaces;

namespace RateTheWork.Application.Services.Implementations;

/// <summary>
/// Şirket üyelik yönetimi servisi implementasyonu
/// </summary>
public class CompanySubscriptionService : ICompanySubscriptionService
{
    private readonly IApplicationDbContext _context;

    public CompanySubscriptionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<bool> HasActiveSubscriptionAsync(string companyId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CanPostJobAsync(string companyId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CanReplyToReviewsAsync(string companyId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> CanAuthorizeHRPersonnelAsync
        (string companyId, string personnelId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> IncrementJobPostingCountAsync
        (string subscriptionId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DecrementJobPostingCountAsync
        (string subscriptionId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<CompanySubscriptionStatistics>> GetSubscriptionStatisticsAsync
        (string companyId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<List<UpgradeOption>>> GetUpgradeOptionsAsync
        (string companyId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
