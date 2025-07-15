using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.Company;
using RateTheWork.Domain.Enums.Verification;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Exceptions.CompanyVerificationException;
using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.Interfaces.Services;
using RateTheWork.Domain.Interfaces.Validators;
using RateTheWork.Domain.ValueObjects;

namespace RateTheWork.Domain.Services;

/// <summary>
/// Şirket işlemleri için domain service implementasyonu
/// </summary>
public class CompanyDomainService : ICompanyDomainService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITaxNumberValidator _taxNumberValidator;
    private readonly ICompanyDomainValidator _companyValidator;

    public CompanyDomainService(
        IUnitOfWork unitOfWork,
        ITaxNumberValidator taxNumberValidator,
        ICompanyDomainValidator companyValidator)
    {
        _unitOfWork = unitOfWork;
        _taxNumberValidator = taxNumberValidator;
        _companyValidator = companyValidator;
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
                    //TODO: Verif metodunu kontol et Arguments mismatch hatası var
                    company.Verify(
                        "SYSTEM",
                        verificationMethod: VerificationMethod.TaxNumber.ToString(),
                        verificationNotes: "Vergi Numarası numarası GİB üzerinden doğrulandı",
                        new Dictionary<string, object> { ["Method"] = "TaxNumber" });
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
                    //TODO: verify metodunu kontol et
                    company.Verify(verifiedBy: "SYSTEM",
                        verificationMethod: VerificationMethod.Mersis.ToString(),
                        verificationNotes: "Mersis numarası MERSİS üzerinden doğrulandı",
                        metadata: new Dictionary<string, object> { ["MersisNo"] = company.MersisNo});
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
                    //TODO: verify metodunu kontol et 4 parametre istiyor
                    company.Verify(verifiedBy: "SYSTEM",
                        verificationMethod: VerificationMethod.WebDomain.ToString(),
                        verificationNotes: "Web sitesi domain üzerinden doğrulandı",
                        metadata: new Dictionary<string, object> { ["Domain"] = company.WebsiteUrl });
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
    public async Task<bool> ValidateCompanyInformationAsync(Company company)
    {
        var validationResult = await _companyValidator.ValidateAsync(company.ToString() ?? string.Empty);
        return validationResult.IsValid;
    }
    public async Task<CompanyRiskScore> CalculateCompanyRiskScoreAsync(string companyId)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null)
            throw new EntityNotFoundException(nameof(Company), companyId);

        var reviews = await _unitOfWork.Reviews.GetReviewsByCompanyAsync(companyId, 1, int.MaxValue);
        var reports = await _unitOfWork.Reports.GetAsync(r => r.TargetId == companyId && r.TargetType == "Company");

        // Risk faktörleri
        var factors = new Dictionary<string, double>();

        // 1. Düşük puan riski
        var avgRating = reviews.Any() ? reviews.Average(r => r.OverallRating) : 0;
        factors["LowRating"] = avgRating < (decimal)2.5 ? (2.5 - (double)avgRating) * 20 : 0;

        // 2. Yüksek şikayet oranı
        var reportRatio = reviews.Any() ? (double)reports.Count() / reviews.Count : 0;
        factors["HighReportRatio"] = Math.Min(reportRatio * 100, 30);

        // 3. Doğrulanmamış şirket riski
        factors["UnverifiedCompany"] = company.ApprovalStatus.ToString() != ApprovalStatus.Approved.ToString() ? 20 : 0;

        // 4. Yeni şirket riski
        var companyAge = (DateTime.UtcNow - company.CreatedAt).TotalDays;
        factors["NewCompany"] = companyAge < 90 ? (90 - companyAge) / 90 * 15 : 0;

        // 5. İnaktif dönem riski
        var lastReview = reviews.OrderByDescending(r => r.CreatedAt).FirstOrDefault();
        if (lastReview != null)
        {
            var daysSinceLastReview = (DateTime.UtcNow - lastReview.CreatedAt).TotalDays;
            factors["InactivePeriod"] = daysSinceLastReview > 180 ? Math.Min(daysSinceLastReview / 30, 15) : 0;
        }

        var totalScore = factors.Values.Sum();
        var riskLevel = totalScore switch
        {
            >= 70 => "High",
            >= 40 => "Medium",
            >= 20 => "Low",
            _ => "Minimal"
        };

        return CompanyRiskScore.Create(totalScore, riskLevel, factors);
    }
    public async Task<CompanyReviewStatistics> CalculateCompanyStatisticsAsync(string companyId)
    {
        var reviews = await _unitOfWork.Reviews.GetReviewsByCompanyAsync(companyId, 1, int.MaxValue);
        var activeReviews = reviews.Where(r => r.IsActive).ToList();

        if (!activeReviews.Any())
        {
            return CompanyReviewStatistics.CreateEmpty();
        }

        var averageRating = activeReviews.Average(r => r.OverallRating);
        var totalReviews = activeReviews.Count;
        var verifiedReviews = activeReviews.Count(r => r.IsDocumentVerified);
            
        var ratingDistribution = activeReviews
            .GroupBy(r => Math.Floor(r.OverallRating))
            .ToDictionary(g => (int)g.Key, g => g.Count());

        var categoryAverages = activeReviews
            .GroupBy(r => r.CommentType)
            .ToDictionary(
                g => g.Key, 
                g => g.Average(r => r.OverallRating)
            );

        return CompanyReviewStatistics.Create(
            averageRating,
            totalReviews,
            verifiedReviews,
            ratingDistribution,
            categoryAverages
        );
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
        
        //TODO: 0 çıkma ihtimailinden dolayı abs metdou işlemiyor
        var similarSizeCompetitors = competitors
            .Where(c => Math.Abs((double)(c.EmployeeCount - company.EmployeeCount)!) <= employeeCountRange)
            .OrderBy(c => Math.Abs((double)(c.EmployeeCount - company.EmployeeCount)!))
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
    
     public async Task<bool> MergeCompaniesAsync(string primaryCompanyId, string secondaryCompanyId, string mergedBy)
        {
            var primaryCompany = await _unitOfWork.Companies.GetByIdAsync(primaryCompanyId);
            var secondaryCompany = await _unitOfWork.Companies.GetByIdAsync(secondaryCompanyId);

            if (primaryCompany == null || secondaryCompany == null)
                throw new EntityNotFoundException("Şirketlerden biri bulunamadı.");

            // Merge işlemi öncesi kontroller
            if (!primaryCompany.IsActive || !secondaryCompany.IsActive)
                throw new CompanyNotActiveException("Sadece aktif şirketler birleştirilebilir.");

            // Transaction başlat
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Yorumları taşı
                var secondaryReviews = await _unitOfWork.Reviews.GetReviewsByCompanyAsync(secondaryCompanyId, 1, int.MaxValue);
                foreach (var review in secondaryReviews)
                {
                    review.UpdateCompanyId(primaryCompanyId);
                    await _unitOfWork.Reviews.UpdateAsync(review);
                }

                // İkincil şirketi pasifleştir
                secondaryCompany.Deactivate($"Merged with {primaryCompany.Name} (ID: {primaryCompanyId})");
                await _unitOfWork.Companies.UpdateAsync(secondaryCompany);

                // Birincil şirkete merge notu ekle
                var mergeMetadata = new Dictionary<string, object>
                {
                    ["MergedCompanyId"] = secondaryCompanyId,
                    ["MergedCompanyName"] = secondaryCompany.Name,
                    ["MergeDate"] = DateTime.UtcNow,
                    ["MergedBy"] = mergedBy,
                    ["TransferredReviews"] = secondaryReviews.Count
                };

                primaryCompany.AddMetadata("CompanyMerge", mergeMetadata);
                await _unitOfWork.Companies.UpdateAsync(primaryCompany);

                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

    // Private helper methods
    private bool IsValidLinkedInUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        var pattern = @"^https?://(www\.)?linkedin\.com/company/[\w-]+/?$";
        return System.Text.RegularExpressions.Regex.IsMatch(url, pattern);
    }
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
