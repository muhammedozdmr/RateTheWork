using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.Interfaces.Services;

namespace RateTheWork.Domain.Services;

//TODO: company entityde verify metadata targettype vs vs gibi proplar yok ayrıca baserepositoryde updateasync yok

/// <summary>
/// Şirket işlemleri için domain service implementasyonu
/// </summary>
public class CompanyDomainService : ICompanyDomainService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITcIdentityValidationService _tcValidationService;

    public CompanyDomainService(
        IUnitOfWork unitOfWork, 
        ITcIdentityValidationService tcValidationService)
    {
        _unitOfWork = unitOfWork;
        _tcValidationService = tcValidationService;
    }

    public async Task<bool> VerifyCompanyAsync(string companyId, string verificationMethod)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null)
            throw new EntityNotFoundException(nameof(Company), companyId);

        switch (verificationMethod)
        {
            case "TaxNumber":
                // Vergi numarası doğrulaması
                if (string.IsNullOrEmpty(company.TaxId))
                    throw new InvalidDomainStateException(nameof(Company), "NoTaxId", "Tax number verification");

                // Vergi dairesi entegrasyonu simülasyonu
                var isValidTaxNumber = await ValidateTaxNumberAsync(company.TaxId);
                if (isValidTaxNumber)
                {
                    company.Verify("Vergi numarası doğrulandı");
                    await _unitOfWork.Companies.UpdateAsync(company);
                }
                return isValidTaxNumber;

            case "TradeRegistry":
                // Ticaret sicil doğrulaması
                if (string.IsNullOrEmpty(company.MersisNo))
                    throw new InvalidDomainStateException(nameof(Company), "NoMersisNo", "Trade registry verification");

                // MERSİS entegrasyonu simülasyonu
                var isValidMersis = await ValidateMersisNumberAsync(company.MersisNo);
                if (isValidMersis)
                {
                    company.Verify("MERSİS numarası doğrulandı");
                    await _unitOfWork.Companies.UpdateAsync(company);
                }
                return isValidMersis;

            case "DomainOwnership":
                // Domain sahipliği doğrulaması
                if (string.IsNullOrEmpty(company.WebsiteUrl))
                    throw new InvalidDomainStateException(nameof(Company), "NoWebsite", "Domain verification");

                // DNS doğrulama simülasyonu
                var isDomainVerified = await ValidateDomainOwnershipAsync(company.WebsiteUrl, company.Id);
                if (isDomainVerified)
                {
                    company.Verify("Web sitesi sahipliği doğrulandı");
                    await _unitOfWork.Companies.UpdateAsync(company);
                }
                return isDomainVerified;

            default:
                throw new BusinessRuleException("INVALID_VERIFICATION_METHOD", 
                    $"Geçersiz doğrulama yöntemi: {verificationMethod}");
        }
    }

    public async Task UpdateCompanyStatisticsAsync(string companyId)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null)
            throw new EntityNotFoundException(nameof(Company), companyId);

        // Aktif yorumları getir
        var reviews = await _unitOfWork.Reviews.GetReviewsByCompanyAsync(companyId, 1, int.MaxValue);
        var activeReviews = reviews.Where(r => r.IsActive).ToList();

        if (!activeReviews.Any())
        {
            company.UpdateReviewStatistics(0, 0);
            await _unitOfWork.Companies.UpdateAsync(company);
            return;
        }

        // Ortalama puanı hesapla
        var averageRating = activeReviews.Average(r => r.OverallRating);
        var totalReviews = activeReviews.Count;

        // Kategorilere göre ortalama puanlar
        var categoryAverages = activeReviews
            .GroupBy(r => r.CommentType)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Average = g.Average(r => r.OverallRating),
                    Count = g.Count()
                }
            );

        // Doğrulanmış yorum sayısı
        var verifiedReviewCount = activeReviews.Count(r => r.IsDocumentVerified);

        // İstatistikleri güncelle
        company.UpdateReviewStatistics(averageRating, totalReviews);
        company.UpdatedAt = DateTime.UtcNow;

        // Metadata güncelle (varsa)
        if (company.Metadata == null)
            company.Metadata = new Dictionary<string, object>();

        company.Metadata["CategoryAverages"] = categoryAverages;
        company.Metadata["VerifiedReviewCount"] = verifiedReviewCount;
        company.Metadata["LastStatisticsUpdate"] = DateTime.UtcNow;

        await _unitOfWork.Companies.UpdateAsync(company);
    }

    public async Task<bool> IsCompanyActiveForReviewsAsync(string companyId)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null)
            return false;

        // Onaylı değilse
        if (!company.IsApproved)
            return false;

        // Kara listede mi?
        var blacklisted = await _unitOfWork.Bans
            .GetFirstOrDefaultAsync(b => 
                b.TargetType == "Company" && 
                b.TargetId == companyId && 
                b.IsActive &&
                (b.UnbanDate == null || b.UnbanDate > DateTime.UtcNow));

        if (blacklisted != null)
            return false;

        // Birleşme/kapanma durumu
        if (company.Metadata?.ContainsKey("MergedWithCompanyId") == true)
            return false;

        return true;
    }

    public async Task<Company?> GetMergedCompanyAsync(string companyId)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null)
            return null;

        // Birleşme bilgisi var mı?
        if (company.Metadata?.TryGetValue("MergedWithCompanyId", out var mergedId) == true)
        {
            var mergedCompanyId = mergedId?.ToString();
            if (!string.IsNullOrEmpty(mergedCompanyId))
            {
                return await _unitOfWork.Companies.GetByIdAsync(mergedCompanyId);
            }
        }

        return null;
    }

    public async Task<CompanyRiskScore> CalculateCompanyRiskScoreAsync(string companyId)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null)
            throw new EntityNotFoundException(nameof(Company), companyId);

        var riskScore = new CompanyRiskScore();
        var riskFactors = new List<string>();

        // 1. Finansal Risk (Vergi borcu kontrolü simülasyonu)
        if (string.IsNullOrEmpty(company.TaxId))
        {
            riskScore.FinancialRisk = 0.3m;
            riskFactors.Add("Vergi numarası eksik");
        }

        // 2. İtibar Riski (Kötü yorumlar)
        var reviews = await _unitOfWork.Reviews.GetReviewsByCompanyAsync(companyId, 1, int.MaxValue);
        var avgRating = reviews.Any() ? reviews.Average(r => r.OverallRating) : 0;
        
        if (avgRating < 2.5m && reviews.Count > 10)
        {
            riskScore.ReputationalRisk = 0.8m;
            riskFactors.Add("Düşük ortalama puan");
        }
        else if (avgRating < 3.5m)
        {
            riskScore.ReputationalRisk = 0.4m;
            riskFactors.Add("Orta seviye ortalama puan");
        }

        // 3. Uyumluluk Riski
        if (!company.IsApproved)
        {
            riskScore.ComplianceRisk = 0.5m;
            riskFactors.Add("Onaylanmamış şirket");
        }

        // Raporlanma sayısı
        var reports = await _unitOfWork.Reports
            .GetAsync(r => r.TargetType == "Company" && r.TargetId == companyId);
        
        if (reports.Count > 5)
        {
            riskScore.ComplianceRisk = Math.Max(riskScore.ComplianceRisk, 0.6m);
            riskFactors.Add($"{reports.Count} şikayet var");
        }

        // Genel risk hesaplama
        riskScore.OverallRisk = (riskScore.FinancialRisk + riskScore.ReputationalRisk + riskScore.ComplianceRisk) / 3;
        riskScore.RiskFactors = riskFactors;
        
        // Risk seviyesi belirleme
        if (riskScore.OverallRisk < 0.3m)
            riskScore.RiskLevel = "Low";
        else if (riskScore.OverallRisk < 0.6m)
            riskScore.RiskLevel = "Medium";
        else
            riskScore.RiskLevel = "High";

        return riskScore;
    }

    public async Task<CompanyCategory> DetermineCompanyCategoryAsync(string companyId)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null)
            throw new EntityNotFoundException(nameof(Company), companyId);

        var category = new CompanyCategory
        {
            MainCategory = company.Sector
        };

        // Alt kategoriler (sektöre göre)
        switch (company.Sector?.ToLowerInvariant())
        {
            case "teknoloji":
                category.SubCategories = new List<string> { "Yazılım", "Donanım", "E-ticaret", "Oyun" };
                break;
            case "finans":
                category.SubCategories = new List<string> { "Bankacılık", "Sigorta", "Yatırım", "Fintech" };
                break;
            case "sağlık":
                category.SubCategories = new List<string> { "Hastane", "İlaç", "Medikal Cihaz", "Sağlık Teknolojisi" };
                break;
            default:
                category.SubCategories = new List<string> { company.Sector ?? "Diğer" };
                break;
        }

        // Şirket büyüklüğü (çalışan sayısına göre)
        if (company.EmployeeCount < 10)
            category.CompanySize = "Micro";
        else if (company.EmployeeCount < 50)
            category.CompanySize = "Small";
        else if (company.EmployeeCount < 250)
            category.CompanySize = "Medium";
        else if (company.EmployeeCount < 1000)
            category.CompanySize = "Large";
        else
            category.CompanySize = "Enterprise";

        // Olgunluk seviyesi (kuruluş tarihine göre)
        var age = (DateTime.UtcNow - company.CreatedAt).TotalDays / 365;
        if (age < 2)
            category.MaturityLevel = "Startup";
        else if (age < 5)
            category.MaturityLevel = "Growing";
        else if (age < 15)
            category.MaturityLevel = "Mature";
        else
            category.MaturityLevel = "Established";

        return category;
    }

    public async Task<List<Company>> FindCompetitorCompaniesAsync(string companyId, int maxResults = 10)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null)
            throw new EntityNotFoundException(nameof(Company), companyId);

        // Aynı sektördeki şirketler
        var competitors = await _unitOfWork.Companies
            .GetAsync(c => 
                c.Id != companyId && 
                c.Sector == company.Sector && 
                c.IsApproved);

        // Benzer büyüklükteki şirketleri önceliklendir
        var employeeCountRange = company.EmployeeCount * 0.5;
        var similarSizeCompetitors = competitors
            .Where(c => Math.Abs(c.EmployeeCount - company.EmployeeCount) <= employeeCountRange)
            .OrderBy(c => Math.Abs(c.EmployeeCount - company.EmployeeCount))
            .Take(maxResults)
            .ToList();

        // Eğer yeterli sayıda yoksa, tüm sektörden al
        if (similarSizeCompetitors.Count < maxResults)
        {
            var remaining = competitors
                .Where(c => !similarSizeCompetitors.Contains(c))
                .OrderByDescending(c => c.ReviewStatistics.TotalReviews)
                .Take(maxResults - similarSizeCompetitors.Count);
            
            similarSizeCompetitors.AddRange(remaining);
        }

        return similarSizeCompetitors;
    }

    public async Task<CompanyGrowthAnalysis> AnalyzeCompanyGrowthAsync(string companyId, DateTime startDate, DateTime endDate)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null)
            throw new EntityNotFoundException(nameof(Company), companyId);

        var analysis = new CompanyGrowthAnalysis();

        // Belirtilen dönemdeki yorumları al
        var reviews = await _unitOfWork.Reviews
            .GetAsync(r => 
                r.CompanyId == companyId && 
                r.CreatedAt >= startDate && 
                r.CreatedAt <= endDate &&
                r.IsActive);

        // Önceki dönem için karşılaştırma
        var periodLength = endDate - startDate;
        var previousStartDate = startDate.Subtract(periodLength);
        var previousEndDate = startDate.AddDays(-1);

        var previousReviews = await _unitOfWork.Reviews
            .GetAsync(r => 
                r.CompanyId == companyId && 
                r.CreatedAt >= previousStartDate && 
                r.CreatedAt <= previousEndDate &&
                r.IsActive);

        // Yorum büyüme oranı
        if (previousReviews.Any())
        {
            analysis.ReviewGrowthRate = ((decimal)reviews.Count - previousReviews.Count) / previousReviews.Count * 100;
        }
        else
        {
            analysis.ReviewGrowthRate = reviews.Any() ? 100 : 0;
        }

        // Puan trendi
        var currentAvgRating = reviews.Any() ? reviews.Average(r => r.OverallRating) : 0;
        var previousAvgRating = previousReviews.Any() ? previousReviews.Average(r => r.OverallRating) : 0;
        analysis.RatingTrend = currentAvgRating - previousAvgRating;

        // Kategori bazlı büyüme
        var categoryGroups = reviews.GroupBy(r => r.CommentType);
        var previousCategoryGroups = previousReviews.GroupBy(r => r.CommentType).ToDictionary(g => g.Key, g => g.Count());

        foreach (var category in categoryGroups)
        {
            var currentCount = category.Count();
            var previousCount = previousCategoryGroups.GetValueOrDefault(category.Key, 0);
            
            if (previousCount > 0)
            {
                analysis.CategoryGrowthRates[category.Key] = ((decimal)currentCount - previousCount) / previousCount * 100;
            }
            else
            {
                analysis.CategoryGrowthRates[category.Key] = currentCount > 0 ? 100 : 0;
            }
        }

        // Büyüme göstergeleri
        if (analysis.ReviewGrowthRate > 50)
            analysis.GrowthIndicators.Add("Hızlı yorum artışı");
        
        if (analysis.RatingTrend > 0.5m)
            analysis.GrowthIndicators.Add("Puan ortalaması yükseliyor");
        
        if (analysis.CategoryGrowthRates.Any(c => c.Value > 100))
            analysis.GrowthIndicators.Add("Bazı kategorilerde %100+ büyüme");

        // Büyüme fazı belirleme
        if (analysis.ReviewGrowthRate > 100)
            analysis.GrowthPhase = "Rapid Growth";
        else if (analysis.ReviewGrowthRate > 20)
            analysis.GrowthPhase = "Growth";
        else if (analysis.ReviewGrowthRate > -20)
            analysis.GrowthPhase = "Stable";
        else
            analysis.GrowthPhase = "Decline";

        return analysis;
    }

    // Private helper methods
    private async Task<bool> ValidateTaxNumberAsync(string taxNumber)
    {
        // Gerçek uygulamada vergi dairesi API'si çağrılacak
        await Task.Delay(100); // Simülasyon
        
        // Basit format kontrolü (10 veya 11 haneli)
        return !string.IsNullOrEmpty(taxNumber) && 
               (taxNumber.Length == 10 || taxNumber.Length == 11) && 
               taxNumber.All(char.IsDigit);
    }

    private async Task<bool> ValidateMersisNumberAsync(string mersisNo)
    {
        // Gerçek uygulamada MERSİS API'si çağrılacak
        await Task.Delay(100); // Simülasyon
        
        // MERSİS no format: 16 haneli
        return !string.IsNullOrEmpty(mersisNo) && 
               mersisNo.Length == 16 && 
               mersisNo.All(char.IsDigit);
    }

    private async Task<bool> ValidateDomainOwnershipAsync(string websiteUrl, string companyId)
    {
        // Gerçek uygulamada DNS TXT kaydı kontrolü yapılacak
        // Örnek: ratethework-verify=companyId
        await Task.Delay(100); // Simülasyon
        
        // URL format kontrolü
        return !string.IsNullOrEmpty(websiteUrl) && 
               Uri.TryCreate(websiteUrl, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
