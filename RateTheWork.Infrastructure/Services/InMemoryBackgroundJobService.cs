using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;

namespace RateTheWork.Infrastructure.Services;

/// <summary>
/// In-memory background job service for development environment
/// </summary>
public class InMemoryBackgroundJobService : IBackgroundJobService
{
    private readonly ILogger<InMemoryBackgroundJobService> _logger;

    public InMemoryBackgroundJobService(ILogger<InMemoryBackgroundJobService> logger)
    {
        _logger = logger;
    }

    public string Enqueue(Expression<Func<Task>> methodCall)
    {
        _logger.LogDebug("Job enqueued (in-memory): {Method}", methodCall.ToString());
        return Guid.NewGuid().ToString();
    }

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
    {
        _logger.LogDebug("Job enqueued (in-memory): {Method}", methodCall.ToString());
        return Guid.NewGuid().ToString();
    }

    public string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay)
    {
        _logger.LogDebug("Job scheduled (in-memory) for {Delay}: {Method}", delay, methodCall.ToString());
        return Guid.NewGuid().ToString();
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay)
    {
        _logger.LogDebug("Job scheduled (in-memory) for {Delay}: {Method}", delay, methodCall.ToString());
        return Guid.NewGuid().ToString();
    }

    public string Schedule(Expression<Func<Task>> methodCall, DateTime enqueueAt)
    {
        _logger.LogDebug("Job scheduled (in-memory) for {EnqueueAt}: {Method}", enqueueAt, methodCall.ToString());
        return Guid.NewGuid().ToString();
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTime enqueueAt)
    {
        _logger.LogDebug("Job scheduled (in-memory) for {EnqueueAt}: {Method}", enqueueAt, methodCall.ToString());
        return Guid.NewGuid().ToString();
    }

    public void RecurringJob(string jobId, Expression<Func<Task>> methodCall, string cronExpression)
    {
        _logger.LogDebug("Recurring job added (in-memory): {JobId} with cron {Cron}", jobId, cronExpression);
    }

    public void RecurringJob<T>(string jobId, Expression<Func<T, Task>> methodCall, string cronExpression)
    {
        _logger.LogDebug("Recurring job added (in-memory): {JobId} with cron {Cron}", jobId, cronExpression);
    }

    public void DeleteRecurringJob(string jobId)
    {
        _logger.LogDebug("Recurring job deleted (in-memory): {JobId}", jobId);
    }

    public bool Delete(string jobId)
    {
        _logger.LogDebug("Job deleted (in-memory): {JobId}", jobId);
        return true;
    }

    public bool Requeue(string jobId)
    {
        _logger.LogDebug("Job requeued (in-memory): {JobId}", jobId);
        return true;
    }

    public string ContinueWith(string parentJobId, Expression<Func<Task>> methodCall)
    {
        _logger.LogDebug("Continuation job created (in-memory) for parent {ParentJobId}", parentJobId);
        return Guid.NewGuid().ToString();
    }

    public string ContinueWith<T>(string parentJobId, Expression<Func<T, Task>> methodCall)
    {
        _logger.LogDebug("Continuation job created (in-memory) for parent {ParentJobId}", parentJobId);
        return Guid.NewGuid().ToString();
    }
}