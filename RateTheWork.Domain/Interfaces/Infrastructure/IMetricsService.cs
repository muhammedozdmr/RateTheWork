namespace RateTheWork.Domain.Interfaces.Infrastructure;

/// <summary>
/// Metrik toplama service interface'i
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Counter artırır
    /// </summary>
    void IncrementCounter(string name, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Gauge değeri kaydeder
    /// </summary>
    void RecordGauge(string name, double value, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Histogram değeri kaydeder
    /// </summary>
    void RecordHistogram(string name, double value, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Timer başlatır
    /// </summary>
    IDisposable StartTimer(string name, Dictionary<string, string>? tags = null);
}
