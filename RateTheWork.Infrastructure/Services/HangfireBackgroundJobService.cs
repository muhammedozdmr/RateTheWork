using System.Linq.Expressions;
using Hangfire;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;

namespace RateTheWork.Infrastructure.Services;

public class HangfireBackgroundJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<HangfireBackgroundJobService> _logger;
    private readonly IRecurringJobManager _recurringJobManager;

    public HangfireBackgroundJobService
    (
        ILogger<HangfireBackgroundJobService> logger
        , IBackgroundJobClient backgroundJobClient
        , IRecurringJobManager recurringJobManager
    )
    {
        _logger = logger;
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
    }

    public string Enqueue(Expression<Func<Task>> methodCall)
    {
        var jobId = _backgroundJobClient.Enqueue(methodCall);
        _logger.LogInformation("Enqueued job with ID: {JobId}", jobId);
        return jobId;
    }

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
    {
        var jobId = _backgroundJobClient.Enqueue(methodCall);
        _logger.LogInformation("Enqueued job with ID: {JobId}", jobId);
        return jobId;
    }

    public string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay)
    {
        var jobId = _backgroundJobClient.Schedule(methodCall, delay);
        _logger.LogInformation("Scheduled job with ID: {JobId} to run after {Delay}", jobId, delay);
        return jobId;
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay)
    {
        var jobId = _backgroundJobClient.Schedule(methodCall, delay);
        _logger.LogInformation("Scheduled job with ID: {JobId} to run after {Delay}", jobId, delay);
        return jobId;
    }

    public string Schedule(Expression<Func<Task>> methodCall, DateTime enqueueAt)
    {
        var jobId = _backgroundJobClient.Schedule(methodCall, enqueueAt);
        _logger.LogInformation("Scheduled job with ID: {JobId} to run at {EnqueueAt}", jobId, enqueueAt);
        return jobId;
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTime enqueueAt)
    {
        var jobId = _backgroundJobClient.Schedule(methodCall, enqueueAt);
        _logger.LogInformation("Scheduled job with ID: {JobId} to run at {EnqueueAt}", jobId, enqueueAt);
        return jobId;
    }

    public void RecurringJob(string jobId, Expression<Func<Task>> methodCall, string cronExpression)
    {
        _recurringJobManager.AddOrUpdate(jobId, methodCall, cronExpression, TimeZoneInfo.Utc);
        _logger.LogInformation("Created/Updated recurring job: {JobId} with cron: {CronExpression}", jobId
            , cronExpression);
    }

    public void RecurringJob<T>(string jobId, Expression<Func<T, Task>> methodCall, string cronExpression)
    {
        _recurringJobManager.AddOrUpdate(jobId, methodCall, cronExpression, TimeZoneInfo.Utc);
        _logger.LogInformation("Created/Updated recurring job: {JobId} with cron: {CronExpression}", jobId
            , cronExpression);
    }

    public void DeleteRecurringJob(string jobId)
    {
        _recurringJobManager.RemoveIfExists(jobId);
        _logger.LogInformation("Deleted recurring job: {JobId}", jobId);
    }

    public bool Delete(string jobId)
    {
        var result = _backgroundJobClient.Delete(jobId);
        _logger.LogInformation("Deleted job: {JobId}, Result: {Result}", jobId, result);
        return result;
    }

    public bool Requeue(string jobId)
    {
        var result = _backgroundJobClient.Requeue(jobId);
        _logger.LogInformation("Requeued job: {JobId}, Result: {Result}", jobId, result);
        return result;
    }

    public string ContinueWith(string parentJobId, Expression<Func<Task>> methodCall)
    {
        var jobId = _backgroundJobClient.ContinueJobWith(parentJobId, methodCall);
        _logger.LogInformation("Created continuation job: {JobId} after parent: {ParentJobId}", jobId, parentJobId);
        return jobId;
    }

    public string ContinueWith<T>(string parentJobId, Expression<Func<T, Task>> methodCall)
    {
        var jobId = _backgroundJobClient.ContinueJobWith(parentJobId, methodCall);
        _logger.LogInformation("Created continuation job: {JobId} after parent: {ParentJobId}", jobId, parentJobId);
        return jobId;
    }
}
