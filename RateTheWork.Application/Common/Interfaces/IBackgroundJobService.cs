using System.Linq.Expressions;

namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// Background job service interface
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Fire and forget job
    /// </summary>
    string Enqueue(Expression<Func<Task>> methodCall);

    /// <summary>
    /// Fire and forget job with generic
    /// </summary>
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);

    /// <summary>
    /// Delayed job
    /// </summary>
    string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay);

    /// <summary>
    /// Delayed job with generic
    /// </summary>
    string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);

    /// <summary>
    /// Scheduled job at specific time
    /// </summary>
    string Schedule(Expression<Func<Task>> methodCall, DateTime enqueueAt);

    /// <summary>
    /// Scheduled job at specific time with generic
    /// </summary>
    string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTime enqueueAt);

    /// <summary>
    /// Recurring job
    /// </summary>
    void RecurringJob(string jobId, Expression<Func<Task>> methodCall, string cronExpression);

    /// <summary>
    /// Recurring job with generic
    /// </summary>
    void RecurringJob<T>(string jobId, Expression<Func<T, Task>> methodCall, string cronExpression);

    /// <summary>
    /// Delete recurring job
    /// </summary>
    void DeleteRecurringJob(string jobId);

    /// <summary>
    /// Delete job
    /// </summary>
    bool Delete(string jobId);

    /// <summary>
    /// Requeue job
    /// </summary>
    bool Requeue(string jobId);

    /// <summary>
    /// Continuation job
    /// </summary>
    string ContinueWith(string parentJobId, Expression<Func<Task>> methodCall);

    /// <summary>
    /// Continuation job with generic
    /// </summary>
    string ContinueWith<T>(string parentJobId, Expression<Func<T, Task>> methodCall);
}

/// <summary>
/// Background job options
/// </summary>
public class BackgroundJobOptions
{
    /// <summary>
    /// Job queue name
    /// </summary>
    public string Queue { get; set; } = "default";

    /// <summary>
    /// Job priority
    /// </summary>
    public JobPriority Priority { get; set; } = JobPriority.Normal;

    /// <summary>
    /// Retry attempts
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Job timeout in minutes
    /// </summary>
    public int TimeoutMinutes { get; set; } = 30;
}

/// <summary>
/// Job priority levels
/// </summary>
public enum JobPriority
{
    Low = 0
    , Normal = 1
    , High = 2
    , Critical = 3
}

/// <summary>
/// Recurring job options
/// </summary>
public class RecurringJobOptions : BackgroundJobOptions
{
    /// <summary>
    /// Time zone for cron expression
    /// </summary>
    public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Utc;

    /// <summary>
    /// Skip execution if previous run is still active
    /// </summary>
    public bool SkipIfStillRunning { get; set; } = true;
}
