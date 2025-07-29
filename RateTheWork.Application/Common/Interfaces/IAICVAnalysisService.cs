using RateTheWork.Domain.ValueObjects.CV;

namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// AI destekli CV analiz servisi interface'i
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