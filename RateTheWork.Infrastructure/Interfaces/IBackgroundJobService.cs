using System.Linq.Expressions;

namespace RateTheWork.Infrastructure.Interfaces;

/// <summary>
/// Background job service interface'i
/// Hangfire veya benzeri job scheduler'lar ile implemente edilir.
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Job'ı hemen çalıştırır
    /// </summary>
    Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> methodCall);
    
    /// <summary>
    /// Job'ı zamanlanmış olarak ekler
    /// </summary>
    Task<string> ScheduleAsync<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);
    
    /// <summary>
    /// Job'ı belirli bir tarihte çalıştırır
    /// </summary>
    Task<string> ScheduleAsync<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt);
    
    /// <summary>
    /// Tekrarlı job ekler
    /// </summary>
    Task AddRecurringAsync<T>(string jobId, Expression<Func<T, Task>> methodCall, string cronExpression);
    
    /// <summary>
    /// Job'ı iptal eder
    /// </summary>
    Task<bool> DeleteAsync(string jobId);
    
    /// <summary>
    /// Tekrarlı job'ı siler
    /// </summary>
    Task<bool> RemoveRecurringAsync(string jobId);
    
    /// <summary>
    /// Job durumunu sorgular
    /// </summary>
    Task<JobStatus> GetJobStatusAsync(string jobId);
}

/// <summary>
/// Job durumu
/// </summary>
public enum JobStatus
{
    Scheduled,
    Enqueued,
    Processing,
    Succeeded,
    Failed,
    Deleted
}