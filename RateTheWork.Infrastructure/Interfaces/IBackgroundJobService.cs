using System.Linq.Expressions;

namespace RateTheWork.Infrastructure.Interfaces;

/// <summary>
/// Background job service interface'i
/// Hangfire veya benzeri job scheduler'lar ile implemente edilir.
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// İşi hemen çalıştırır
    /// </summary>
    Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> methodCall);

    /// <summary>
    /// İşi zamanlanmış olarak ekler
    /// </summary>
    Task<string> ScheduleAsync<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);

    /// <summary>
    /// İşi belirli bir tarihte çalıştırır
    /// </summary>
    Task<string> ScheduleAsync<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt);

    /// <summary>
    /// Tekrarlı iş ekler
    /// </summary>
    Task AddRecurringAsync<T>(string jobId, Expression<Func<T, Task>> methodCall, string cronExpression);

    /// <summary>
    /// İşi iptal eder
    /// </summary>
    Task<bool> DeleteAsync(string jobId);

    /// <summary>
    /// Tekrarlı işi siler
    /// </summary>
    Task<bool> RemoveRecurringAsync(string jobId);

    /// <summary>
    /// İş durumunu sorgular
    /// </summary>
    Task<JobStatus> GetJobStatusAsync(string jobId);
}

/// <summary>
/// İş durumu
/// </summary>
public enum JobStatus
{
    Scheduled
    , Enqueued
    , Processing
    , Succeeded
    , Failed
    , Deleted
}
