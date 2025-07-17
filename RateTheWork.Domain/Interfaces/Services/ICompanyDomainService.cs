using RateTheWork.Domain.Entities;
using RateTheWork.Domain.ValueObjects.Company;

namespace RateTheWork.Domain.Interfaces.Services;

/// <summary>
/// Şirket işlemleri için domain service interface'i
/// </summary>
public interface ICompanyDomainService
{
    /// <summary>
    /// Şirket doğrulama işlemi
    /// </summary>
    Task<bool> VerifyCompanyAsync(string companyId, string verificationMethod);

    /// <summary>
    /// Şirket istatistiklerini günceller
    /// </summary>
    Task UpdateCompanyStatisticsAsync(string companyId);

    /// <summary>
    /// Şirket aktiflik durumunu kontrol eder
    /// </summary>
    Task<bool> IsCompanyActiveForReviewsAsync(string companyId);

    /// <summary>
    /// Şirket birleşme/satın alma durumunu kontrol eder
    /// </summary>
    Task<Company?> GetMergedCompanyAsync(string companyId);

    /// <summary>
    /// Şirket risk skorunu hesaplar
    /// </summary>
    Task<CompanyRiskScore> CalculateCompanyRiskScoreAsync(string companyId);

    /// <summary>
    /// Şirket kategorisini belirler
    /// </summary>
    Task<CompanyCategory> DetermineCompanyCategoryAsync(string companyId);

    /// <summary>
    /// Rakip şirketleri bulur
    /// </summary>
    Task<List<Company>> FindCompetitorCompaniesAsync(string companyId, int maxResults = 10);

    /// <summary>
    /// Şirket büyüme trendini analiz eder
    /// </summary>
    Task<CompanyGrowthAnalysis> AnalyzeCompanyGrowthAsync(string companyId, DateTime startDate, DateTime endDate);
}
