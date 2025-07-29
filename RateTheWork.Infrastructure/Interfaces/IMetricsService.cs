namespace RateTheWork.Infrastructure.Interfaces;

/// <summary>
/// Metrik toplama servisi arayüzü
/// Prometheus, ApplicationInsights veya benzeri sistemlerle uygulanır.
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Sayaç artırır
    /// </summary>
    void IncrementCounter(string name, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Sayacı belirli bir değer kadar artırır
    /// </summary>
    void IncrementCounter(string name, double value, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Gösterge değeri kaydeder
    /// </summary>
    void RecordGauge(string name, double value, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Histogram değeri kaydeder
    /// </summary>
    void RecordHistogram(string name, double value, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Zamanlayıcı başlatır
    /// </summary>
    IDisposable StartTimer(string name, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Özet değeri kaydeder
    /// </summary>
    void RecordSummary(string name, double value, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Özel olay kaydeder
    /// </summary>
    void TrackEvent
        (string name, Dictionary<string, string>? properties = null, Dictionary<string, double>? metrics = null);

    /// <summary>
    /// Hata kaydeder
    /// </summary>
    void TrackException
    (
        Exception exception
        , Dictionary<string, string>? properties = null
        , Dictionary<string, double>? metrics = null
    );
}
