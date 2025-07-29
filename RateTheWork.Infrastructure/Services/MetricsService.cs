using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;

namespace RateTheWork.Infrastructure.Services;

/// <summary>
/// Uygulama metriklerini toplamak ve raporlamak için servis
/// OpenTelemetry uyumlu metrikler sağlar
/// </summary>
public class MetricsService : IMetricsService
{
    // Göstergeler
    private readonly ObservableGauge<int> _activeUsersGauge;
    private readonly Histogram<double> _cacheOperationDuration;
    private readonly Histogram<double> _databaseQueryDuration;
    private readonly Counter<long> _emailSentCounter;
    private readonly Counter<long> _errorCounter;
    private readonly ILogger<MetricsService> _logger;
    private readonly Counter<long> _loginCounter;
    private readonly Meter _meter;
    private readonly ObservableGauge<int> _queueSizeGauge;
    private readonly Dictionary<string, int> _queueSizes = new();
    private readonly Counter<long> _registrationCounter;

    // Sayaçlar
    private readonly Counter<long> _requestCounter;

    // Histogramlar
    private readonly Histogram<double> _requestDuration;
    private readonly Counter<long> _smsSentCounter;

    private int _activeUsers = 0;

    public MetricsService(ILogger<MetricsService> logger)
    {
        _logger = logger;
        _meter = new Meter("RateTheWork.Application", "1.0.0");

        // Sayaçları oluştur
        _requestCounter = _meter.CreateCounter<long>(
            "ratethework_http_requests_total",
            "requests",
            "Toplam HTTP istek sayısı");

        _errorCounter = _meter.CreateCounter<long>(
            "ratethework_errors_total",
            "errors",
            "Toplam hata sayısı");

        _loginCounter = _meter.CreateCounter<long>(
            "ratethework_logins_total",
            "logins",
            "Toplam giriş sayısı");

        _registrationCounter = _meter.CreateCounter<long>(
            "ratethework_registrations_total",
            "registrations",
            "Toplam kayıt sayısı");

        _emailSentCounter = _meter.CreateCounter<long>(
            "ratethework_emails_sent_total",
            "emails",
            "Gönderilen toplam email sayısı");

        _smsSentCounter = _meter.CreateCounter<long>(
            "ratethework_sms_sent_total",
            "sms",
            "Gönderilen toplam SMS sayısı");

        // Histogramları oluştur
        _requestDuration = _meter.CreateHistogram<double>(
            "ratethework_http_request_duration_seconds",
            "seconds",
            "HTTP istek süreleri");

        _databaseQueryDuration = _meter.CreateHistogram<double>(
            "ratethework_database_query_duration_seconds",
            "seconds",
            "Veritabanı sorgu süreleri");

        _cacheOperationDuration = _meter.CreateHistogram<double>(
            "ratethework_cache_operation_duration_seconds",
            "seconds",
            "Önbellek işlem süreleri");

        // Gözlemlenebilir göstergeleri oluştur
        _activeUsersGauge = _meter.CreateObservableGauge<int>(
            "ratethework_active_users",
            () => _activeUsers,
            "users",
            "Aktif kullanıcı sayısı");

        _queueSizeGauge = _meter.CreateObservableGauge<int>(
            "ratethework_queue_size",
            () => GetQueueSizeMetrics(),
            "items",
            "Kuyruk boyutları");
    }

    /// <summary>
    /// İstek sayacını artırır
    /// </summary>
    public void IncrementCounter(string name, Dictionary<string, object?>? tags = null)
    {
        var tagList = CreateTagList(tags);

        switch (name.ToLowerInvariant())
        {
            case "request":
                _requestCounter.Add(1, tagList.ToArray());
                break;
            case "error":
                _errorCounter.Add(1, tagList.ToArray());
                break;
            case "login":
                _loginCounter.Add(1, tagList.ToArray());
                break;
            case "registration":
                _registrationCounter.Add(1, tagList.ToArray());
                break;
            case "email_sent":
                _emailSentCounter.Add(1, tagList.ToArray());
                break;
            case "sms_sent":
                _smsSentCounter.Add(1, tagList.ToArray());
                break;
            default:
                _logger.LogWarning("Unknown counter name: {CounterName}", name);
                break;
        }
    }

    /// <summary>
    /// Süre ölçümü kaydeder
    /// </summary>
    public void RecordDuration(string name, double duration, Dictionary<string, object?>? tags = null)
    {
        var tagList = CreateTagList(tags);

        switch (name.ToLowerInvariant())
        {
            case "request":
                _requestDuration.Record(duration, tagList.ToArray());
                break;
            case "database_query":
                _databaseQueryDuration.Record(duration, tagList.ToArray());
                break;
            case "cache_operation":
                _cacheOperationDuration.Record(duration, tagList.ToArray());
                break;
            default:
                _logger.LogWarning("Unknown histogram name: {HistogramName}", name);
                break;
        }
    }

    /// <summary>
    /// Gösterge değerini ayarlar
    /// </summary>
    public void SetGauge(string name, double value, Dictionary<string, object?>? tags = null)
    {
        switch (name.ToLowerInvariant())
        {
            case "active_users":
                _activeUsers = (int)value;
                break;
            case "queue_size":
                if (tags != null && tags.TryGetValue("queue_name", out var queueName) && queueName != null)
                {
                    _queueSizes[queueName.ToString()!] = (int)value;
                }

                break;
            default:
                _logger.LogWarning("Unknown gauge name: {GaugeName}", name);
                break;
        }
    }

    /// <summary>
    /// Otomatik süre ölçümü başlatır
    /// </summary>
    public IDisposable MeasureDuration(string name, Dictionary<string, object?>? tags = null)
    {
        return new DurationMeasurement(this, name, tags);
    }

    /// <summary>
    /// Özel metrik ekler
    /// </summary>
    public void RecordCustomMetric(string name, double value, string unit, Dictionary<string, object?>? tags = null)
    {
        try
        {
            var tagList = CreateTagList(tags);

            // Özel metrikler için log kullan (OpenTelemetry exporter eklenene kadar)
            _logger.LogInformation("Custom metric recorded: {MetricName}={Value}{Unit} Tags: {Tags}",
                name, value, unit, string.Join(", ", tagList.Select(t => $"{t.Key}={t.Value}")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording custom metric: {MetricName}", name);
        }
    }

    /// <summary>
    /// Etiket listesi oluşturur
    /// </summary>
    private List<KeyValuePair<string, object?>> CreateTagList(Dictionary<string, object?>? tags)
    {
        var tagList = new List<KeyValuePair<string, object?>>();

        if (tags != null)
        {
            foreach (var tag in tags)
            {
                tagList.Add(new KeyValuePair<string, object?>(tag.Key, tag.Value));
            }
        }

        return tagList;
    }

    /// <summary>
    /// Kuyruk boyutu metriklerini getirir
    /// </summary>
    private IEnumerable<Measurement<int>> GetQueueSizeMetrics()
    {
        foreach (var queue in _queueSizes)
        {
            yield return new Measurement<int>(
                queue.Value,
                new KeyValuePair<string, object?>("queue_name", queue.Key));
        }
    }

    /// <summary>
    /// Süre ölçümü için yardımcı sınıf
    /// </summary>
    private class DurationMeasurement : IDisposable
    {
        private readonly MetricsService _metricsService;
        private readonly string _name;
        private readonly Stopwatch _stopwatch;
        private readonly Dictionary<string, object?>? _tags;

        public DurationMeasurement
        (
            MetricsService metricsService
            , string name
            , Dictionary<string, object?>? tags
        )
        {
            _metricsService = metricsService;
            _name = name;
            _tags = tags;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _metricsService.RecordDuration(
                _name,
                _stopwatch.Elapsed.TotalSeconds,
                _tags);
        }
    }
}
