namespace RateTheWork.Infrastructure.Interfaces;

/// <summary>
/// Metrik toplama service interface'i
/// Prometheus, ApplicationInsights veya benzeri sistemlerle implemente edilir.
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Counter artırır
    /// </summary>
    void IncrementCounter(string name, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Counter'ı belirli bir değer kadar artırır
    /// </summary>
    void IncrementCounter(string name, double value, Dictionary<string, string>? tags = null);
    
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
    
    /// <summary>
    /// Summary değeri kaydeder
    /// </summary>
    void RecordSummary(string name, double value, Dictionary<string, string>? tags = null);
    
    /// <summary>
    /// Custom event kaydeder
    /// </summary>
    void TrackEvent(string name, Dictionary<string, string>? properties = null, Dictionary<string, double>? metrics = null);
    
    /// <summary>
    /// Exception kaydeder
    /// </summary>
    void TrackException(Exception exception, Dictionary<string, string>? properties = null, Dictionary<string, double>? metrics = null);
}