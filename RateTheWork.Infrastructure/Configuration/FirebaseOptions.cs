namespace RateTheWork.Infrastructure.Configuration;

/// <summary>
/// Firebase yapılandırma seçenekleri
/// </summary>
public class FirebaseOptions
{
    public const string SectionName = "Firebase";

    /// <summary>
    /// Firebase proje ID'si
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Service account JSON dosyasının yolu veya JSON içeriği
    /// </summary>
    public string ServiceAccountJson { get; set; } = string.Empty;

    /// <summary>
    /// Firebase database URL'i (opsiyonel)
    /// </summary>
    public string? DatabaseUrl { get; set; }

    /// <summary>
    /// Firebase storage bucket (opsiyonel)
    /// </summary>
    public string? StorageBucket { get; set; }
}
