using RateTheWork.Domain.Entities;
using RateTheWork.Domain.ValueObjects;

namespace RateTheWork.Domain.Interfaces.Services;

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
    
    /// <summary>
    /// İçerik özetler
    /// </summary>
    Task<string> SummarizeContentAsync(string content, int maxLength = 200);
    
    /// <summary>
    /// Duygu analizi yapar
    /// </summary>
    Task<SentimentAnalysisResult> AnalyzeSentimentAsync(string content);
    
    /// <summary>
    /// Anahtar kelimeleri çıkarır
    /// </summary>
    Task<List<string>> ExtractKeywordsAsync(string content, int maxKeywords = 10);
    
    /// <summary>
    /// İçeriği kategorize eder
    /// </summary>
    Task<List<ContentCategory>> CategorizeContentAsync(string content);
    
    /// <summary>
    /// Yorum içeriğini kontrol eder
    /// </summary>
    Task<ModerationResult> ModerateReviewAsync(string commentText, string commentType);
        
    /// <summary>
    /// Şirket bilgilerini kontrol eder
    /// </summary>
    Task<ModerationResult> ModerateCompanyInfoAsync(string companyName, string? description = null);
        
    /// <summary>
    /// Kullanıcı adını kontrol eder
    /// </summary>
    Task<ModerationResult> ModerateUsernameAsync(string username);
        
    /// <summary>
    /// Toplu içerik kontrolü yapar
    /// </summary>
    Task<List<ModerationResult>> ModerateBulkAsync(List<string> contents);
        
    /// <summary>
    /// İçeriği temizler ve düzeltir
    /// </summary>
    Task<string> SanitizeContentAsync(string content);
        
    /// <summary>
    /// Spam kontrolü yapar
    /// </summary>
    Task<bool> IsSpamAsync(string content, string userId);
        
    /// <summary>
    /// Kelime filtreleme yapar
    /// </summary>
    List<string> FilterProhibitedWords(string content);
        
    /// <summary>
    /// İçerik skorunu hesaplar
    /// </summary>
    double CalculateContentScore(string content);
}