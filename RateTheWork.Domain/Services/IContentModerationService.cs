namespace RateTheWork.Domain.Services;

/// <summary>
/// İçerik moderasyon servisi - AI ile yorum içeriğini kontrol eder
/// </summary>
public interface IContentModerationService
{
    /// <summary>
    /// Yorum içeriğini moderasyon kurallarına göre kontrol eder
    /// </summary>
    /// <param name="content">Kontrol edilecek içerik</param>
    /// <param name="language">İçeriğin dili (varsayılan: tr)</param>
    /// <returns>Moderasyon sonucu</returns>
    Task<ModerationResult> ModerateContentAsync(string content, string language = "tr");
    
    /// <summary>
    /// İçeriği belirtilen dile çevirir
    /// </summary>
    /// <param name="content">Çevrilecek içerik</param>
    /// <param name="fromLanguage">Kaynak dil</param>
    /// <param name="toLanguage">Hedef dil</param>
    /// <returns>Çevrilmiş içerik</returns>
    Task<string> TranslateContentAsync(string content, string fromLanguage, string toLanguage);
}

/// <summary>
/// Moderasyon sonucu
/// </summary>
public class ModerationResult
{
    /// <summary>
    /// İçerik onaylandı mı?
    /// </summary>
    public bool IsApproved { get; set; }
    
    /// <summary>
    /// Red nedeni (eğer onaylanmadıysa)
    /// </summary>
    public string? RejectionReason { get; set; }
    
    /// <summary>
    /// Toksisite skoru (0-1 arası, 1 en toksik)
    /// </summary>
    public double ToxicityScore { get; set; }
    
    /// <summary>
    /// Tespit edilen sorunlar
    /// </summary>
    public List<string> DetectedIssues { get; set; } = new();
    
    /// <summary>
    /// Detaylı analiz sonuçları
    /// </summary>
    public ModerationDetails? Details { get; set; }
}

/// <summary>
/// Detaylı moderasyon analizi
/// </summary>
public class ModerationDetails
{
    /// <summary>
    /// Hakaret/Küfür skoru
    /// </summary>
    public double ProfanityScore { get; set; }
    
    /// <summary>
    /// Nefret söylemi skoru
    /// </summary>
    public double HateSpeechScore { get; set; }
    
    /// <summary>
    /// Kişisel saldırı skoru
    /// </summary>
    public double PersonalAttackScore { get; set; }
    
    /// <summary>
    /// Spam skoru
    /// </summary>
    public double SpamScore { get; set; }
    
    /// <summary>
    /// Gizli bilgi paylaşımı skoru
    /// </summary>
    public double ConfidentialInfoScore { get; set; }
    
    /// <summary>
    /// Tespit edilen kişisel bilgiler
    /// </summary>
    public List<string> DetectedPersonalInfo { get; set; } = new();
    
    /// <summary>
    /// Önerilen aksiyonlar
    /// </summary>
    public List<string> SuggestedActions { get; set; } = new();
}
