using RateTheWork.Domain.ValueObjects.CV;

namespace RateTheWork.Domain.Services;

/// <summary>
/// AI destekli CV analiz servisi
/// </summary>
public interface IAICVAnalysisService
{
    /// <summary>
    /// CV'yi analiz eder ve sonuçları döner
    /// </summary>
    Task<CVAnalysisResult> AnalyzeCV(string cvFileUrl, List<string> targetDepartments);
    
    /// <summary>
    /// CV ile pozisyon uyumluluğunu analiz eder
    /// </summary>
    Task<PositionMatchResult> AnalyzePositionMatch(string cvFileUrl, string jobDescription);
    
    /// <summary>
    /// CV'den temel bilgileri çıkarır
    /// </summary>
    Task<CVBasicInfo> ExtractBasicInfo(string cvFileUrl);
}

/// <summary>
/// AI CV analiz servisi implementasyonu
/// </summary>
public class AICVAnalysisService : IAICVAnalysisService
{
    // Bu implementasyon infrastructure katmanında yapılacak
    // Domain katmanında sadece interface ve value object'ler tanımlanır
    
    public Task<CVAnalysisResult> AnalyzeCV(string cvFileUrl, List<string> targetDepartments)
    {
        throw new NotImplementedException("Bu metod Infrastructure katmanında implement edilmelidir.");
    }

    public Task<PositionMatchResult> AnalyzePositionMatch(string cvFileUrl, string jobDescription)
    {
        throw new NotImplementedException("Bu metod Infrastructure katmanında implement edilmelidir.");
    }

    public Task<CVBasicInfo> ExtractBasicInfo(string cvFileUrl)
    {
        throw new NotImplementedException("Bu metod Infrastructure katmanında implement edilmelidir.");
    }
}