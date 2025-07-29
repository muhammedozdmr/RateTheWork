using RateTheWork.Domain.Entities;

namespace RateTheWork.Application.Common.Interfaces.Persistence;

/// <summary>
/// Job application repository interface
/// </summary>
public interface IJobApplicationRepository : IGenericRepository<JobApplication>
{
    Task<IReadOnlyList<JobApplication>> GetUserApplicationsAsync(string userId, int pageNumber, int pageSize);
    Task<IReadOnlyList<JobApplication>> GetJobPostingApplicationsAsync(string jobPostingId, int pageNumber, int pageSize);
    Task<JobApplication?> GetUserApplicationForJobAsync(string userId, string jobPostingId);
    Task<int> GetApplicationCountForJobAsync(string jobPostingId);
}