namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// Health check service interface
/// </summary>
public interface IHealthCheckService
{
    /// <summary>
    /// Check system health
    /// </summary>
    Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check specific component health
    /// </summary>
    Task<ComponentHealthResult> CheckComponentHealthAsync
        (string componentName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get system metrics
    /// </summary>
    Task<SystemMetrics> GetSystemMetricsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Health check result
/// </summary>
public class HealthCheckResult
{
    public HealthStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public Dictionary<string, ComponentHealthResult> Components { get; set; } = new();
}

/// <summary>
/// Component health result
/// </summary>
public class ComponentHealthResult
{
    public string ComponentName { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public TimeSpan ResponseTime { get; set; }
    public Dictionary<string, object>? Details { get; set; }
}

/// <summary>
/// Health status
/// </summary>
public enum HealthStatus
{
    Healthy
    , Degraded
    , Unhealthy
}

/// <summary>
/// System metrics
/// </summary>
public class SystemMetrics
{
    public double CpuUsage { get; set; }
    public long MemoryUsed { get; set; }
    public long MemoryTotal { get; set; }
    public long DiskUsed { get; set; }
    public long DiskTotal { get; set; }
    public int ActiveConnections { get; set; }
    public int RequestsPerSecond { get; set; }
    public double AverageResponseTime { get; set; }
    public Dictionary<string, long> CacheStatistics { get; set; } = new();
    public Dictionary<string, int> QueueStatistics { get; set; } = new();
}
