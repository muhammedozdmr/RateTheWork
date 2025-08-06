namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// Uygulama metriklerini toplamak için servis
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Counter metriğini artırır
    /// </summary>
    /// <param name="name">Metrik adı</param>
    /// <param name="tags">Opsiyonel tag'ler</param>
    void IncrementCounter(string name, Dictionary<string, object?>? tags = null);

    /// <summary>
    /// Süre metriğini kaydeder
    /// </summary>
    /// <param name="name">Metrik adı</param>
    /// <param name="duration">Süre (saniye)</param>
    /// <param name="tags">Opsiyonel tag'ler</param>
    void RecordDuration(string name, double duration, Dictionary<string, object?>? tags = null);

    /// <summary>
    /// Gauge metriğini ayarlar
    /// </summary>
    /// <param name="name">Metrik adı</param>
    /// <param name="value">Değer</param>
    /// <param name="tags">Opsiyonel tag'ler</param>
    void SetGauge(string name, double value, Dictionary<string, object?>? tags = null);

    /// <summary>
    /// Otomatik süre ölçümü başlatır
    /// </summary>
    /// <param name="name">Metrik adı</param>
    /// <param name="tags">Opsiyonel tag'ler</param>
    /// <returns>Dispose edildiğinde süreyi kaydeden IDisposable</returns>
    IDisposable MeasureDuration(string name, Dictionary<string, object?>? tags = null);

    /// <summary>
    /// Custom metrik kaydeder
    /// </summary>
    /// <param name="name">Metrik adı</param>
    /// <param name="value">Değer</param>
    /// <param name="unit">Birim</param>
    /// <param name="tags">Opsiyonel tag'ler</param>
    void RecordCustomMetric(string name, double value, string unit, Dictionary<string, object?>? tags = null);
    
    /// <summary>
    /// Counter metriğini async olarak artırır
    /// </summary>
    /// <param name="name">Metrik adı</param>
    /// <param name="value">Artırılacak değer</param>
    /// <param name="tags">Opsiyonel tag'ler</param>
    Task IncrementCounterAsync(string name, int value = 1, Dictionary<string, object?>? tags = null);
    
    /// <summary>
    /// Blockchain metrikleri için özel metod
    /// </summary>
    /// <param name="operation">İşlem tipi (identity_created, review_stored, etc.)</param>
    /// <param name="success">İşlem başarılı mı</param>
    /// <param name="duration">İşlem süresi (ms)</param>
    Task RecordBlockchainOperationAsync(string operation, bool success, double duration);
}
