using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using RateTheWork.Infrastructure.Interfaces;

namespace RateTheWork.Infrastructure.Persistence.Interceptors;

public class PerformanceInterceptor : DbCommandInterceptor
{
    private readonly ILogger<PerformanceInterceptor> _logger;
    private readonly IMetricsService _metricsService;
    private readonly Stopwatch _stopwatch = new();

    public PerformanceInterceptor
    (
        ILogger<PerformanceInterceptor> logger
        , IMetricsService metricsService
    )
    {
        _logger = logger;
        _metricsService = metricsService;
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting
    (
        DbCommand command
        , CommandEventData eventData
        , InterceptionResult<DbDataReader> result
    )
    {
        _stopwatch.Restart();
        return base.ReaderExecuting(command, eventData, result);
    }

    public override DbDataReader ReaderExecuted
    (
        DbCommand command
        , CommandExecutedEventData eventData
        , DbDataReader result
    )
    {
        RecordExecution(command, eventData);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync
    (
        DbCommand command
        , CommandEventData eventData
        , InterceptionResult<DbDataReader> result
        , CancellationToken cancellationToken = default
    )
    {
        _stopwatch.Restart();
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync
    (
        DbCommand command
        , CommandExecutedEventData eventData
        , DbDataReader result
        , CancellationToken cancellationToken = default
    )
    {
        RecordExecution(command, eventData);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> NonQueryExecuting
    (
        DbCommand command
        , CommandEventData eventData
        , InterceptionResult<int> result
    )
    {
        _stopwatch.Restart();
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override int NonQueryExecuted
    (
        DbCommand command
        , CommandExecutedEventData eventData
        , int result
    )
    {
        RecordExecution(command, eventData);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync
    (
        DbCommand command
        , CommandEventData eventData
        , InterceptionResult<int> result
        , CancellationToken cancellationToken = default
    )
    {
        _stopwatch.Restart();
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override ValueTask<int> NonQueryExecutedAsync
    (
        DbCommand command
        , CommandExecutedEventData eventData
        , int result
        , CancellationToken cancellationToken = default
    )
    {
        RecordExecution(command, eventData);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void RecordExecution(DbCommand command, CommandExecutedEventData eventData)
    {
        _stopwatch.Stop();
        var elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;

        // Yavaş sorguları kaydet
        if (elapsedMilliseconds > 500)
        {
            _logger.LogWarning("Slow query detected ({ElapsedMs}ms): {CommandText}",
                elapsedMilliseconds, command.CommandText);
        }

        // Metrikleri kaydet
        var tags = new Dictionary<string, string>
        {
            ["command_type"] = command.CommandType.ToString(), ["has_error"] = (eventData.Result == null).ToString()
        };

        _metricsService.RecordHistogram("database_query_duration_ms", elapsedMilliseconds, tags);
        _metricsService.IncrementCounter("database_queries_total", tags);
    }
}
