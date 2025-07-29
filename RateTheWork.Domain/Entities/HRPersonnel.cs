using RateTheWork.Domain.Common;
using RateTheWork.Domain.Events.HRPersonnel;
using RateTheWork.Domain.Exceptions.HRPersonnelException;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// İK Personeli entity'si - Şirketlerin İK çalışanları (Anonim değil, şeffaf)
/// </summary>
public class HRPersonnel : AuditableBaseEntity
{
    private HRPersonnel() : base()
    {
    }

    // Kişisel Bilgiler (Şeffaflık için zorunlu)
    public string UserId { get; private set; } = string.Empty; // User entity ile ilişki
    public string CompanyId { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty; // İK Müdürü, İK Uzmanı vb.
    public string Department { get; private set; } = "İnsan Kaynakları";
    public string Email { get; private set; } = string.Empty; // Kurumsal email
    public string? PhoneNumber { get; private set; }
    public string? LinkedInProfile { get; private set; }
    public string? ProfilePhotoUrl { get; private set; }

    // Doğrulama Bilgileri
    public bool IsVerified { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public string? VerificationDocumentUrl { get; private set; }
    public string? VerifiedBy { get; private set; } // Admin ID

    // Yetki ve Durum
    public bool IsActive { get; private set; } = true;
    public bool CanPostJobs { get; private set; } = true;
    public bool CanRespondToReviews { get; private set; } = true;
    public bool CanViewAnalytics { get; private set; } = true;
    public DateTime? DeactivatedAt { get; private set; }
    public string? DeactivationReason { get; private set; }

    // İstatistikler
    public int TotalJobsPosted { get; private set; }
    public int ActiveJobsCount { get; private set; }
    public int TotalResponses { get; private set; }
    public decimal ResponseRate { get; private set; } // Yanıtlama oranı
    public decimal AverageResponseTime { get; private set; } // Ortalama yanıt süresi (saat)
    public decimal TrustScore { get; private set; } = 50; // 0-100 güven skoru

    // İlan Performans Metrikleri
    public int TotalApplicationsReceived { get; private set; }
    public int TotalHiresMade { get; private set; }
    public decimal HiringSuccessRate { get; private set; } // İşe alım başarı oranı
    public decimal AverageTimeToHire { get; private set; } // Ortalama işe alım süresi (gün)
    
    // Application katmanı uyumluluğu için alias'lar
    public int TotalJobPostings => TotalJobsPosted;
    public int TotalHiredCandidates => TotalHiresMade;
    public decimal AverageHiringDays => AverageTimeToHire;

    /// <summary>
    /// Yeni İK personeli oluşturur
    /// </summary>
    public static HRPersonnel Create
    (
        string userId
        , string companyId
        , string firstName
        , string lastName
        , string title
        , string email
        , string? phoneNumber = null
        , string? linkedInProfile = null
    )
    {
        // Validasyonlar
        if (string.IsNullOrWhiteSpace(userId))
            throw new HRPersonnelException("Kullanıcı ID boş olamaz");

        if (string.IsNullOrWhiteSpace(companyId))
            throw new HRPersonnelException("Şirket ID boş olamaz");

        if (string.IsNullOrWhiteSpace(firstName) || firstName.Length < 2)
            throw new HRPersonnelException("Ad en az 2 karakter olmalıdır");

        if (string.IsNullOrWhiteSpace(lastName) || lastName.Length < 2)
            throw new HRPersonnelException("Soyad en az 2 karakter olmalıdır");

        if (string.IsNullOrWhiteSpace(title))
            throw new HRPersonnelException("Ünvan boş olamaz");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            throw new HRPersonnelException("Geçerli bir kurumsal email adresi giriniz");

        var hrPersonnel = new HRPersonnel
        {
            Id = Guid.NewGuid().ToString(), UserId = userId, CompanyId = companyId, FirstName = firstName.Trim()
            , LastName = lastName.Trim(), Title = title.Trim(), Email = email.ToLowerInvariant().Trim()
            , PhoneNumber = phoneNumber?.Trim(), LinkedInProfile = linkedInProfile?.Trim()
        };

        hrPersonnel.AddDomainEvent(new HRPersonnelCreatedEvent(
            hrPersonnel.Id,
            userId,
            companyId,
            $"{firstName} {lastName}"
        ));

        return hrPersonnel;
    }

    /// <summary>
    /// İK personelini doğrular
    /// </summary>
    public void Verify(string adminId, string documentUrl)
    {
        if (IsVerified)
            throw HRPersonnelException.AlreadyVerified(Id);

        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        VerifiedBy = adminId;
        VerificationDocumentUrl = documentUrl;
        TrustScore = 75; // Doğrulanmış personel için başlangıç güven skoru

        AddDomainEvent(new HRPersonnelVerifiedEvent(
            Id,
            CompanyId,
            GetFullName(),
            VerifiedAt.Value
        ));
    }

    /// <summary>
    /// Profil bilgilerini günceller
    /// </summary>
    public void UpdateProfile
    (
        string title
        , string? phoneNumber = null
        , string? linkedInProfile = null
        , string? profilePhotoUrl = null
    )
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new HRPersonnelException("Ünvan boş olamaz");

        Title = title.Trim();
        PhoneNumber = phoneNumber?.Trim();
        LinkedInProfile = linkedInProfile?.Trim();
        ProfilePhotoUrl = profilePhotoUrl?.Trim();
    }

    /// <summary>
    /// Yetkileri günceller
    /// </summary>
    public void UpdatePermissions
    (
        bool canPostJobs
        , bool canRespondToReviews
        , bool canViewAnalytics
    )
    {
        CanPostJobs = canPostJobs;
        CanRespondToReviews = canRespondToReviews;
        CanViewAnalytics = canViewAnalytics;
    }

    /// <summary>
    /// İK personelini deaktif eder
    /// </summary>
    public void Deactivate(string reason)
    {
        if (!IsActive)
            throw HRPersonnelException.NotActive(Id);

        IsActive = false;
        DeactivatedAt = DateTime.UtcNow;
        DeactivationReason = reason;

        // Yetkilerini kaldır
        CanPostJobs = false;
        CanRespondToReviews = false;
        CanViewAnalytics = false;

        AddDomainEvent(new HRPersonnelDeactivatedEvent(
            Id,
            CompanyId,
            GetFullName(),
            DeactivatedAt.Value,
            reason
        ));
    }

    /// <summary>
    /// İK personelini yeniden aktif eder
    /// </summary>
    public void Reactivate()
    {
        if (IsActive)
            throw new HRPersonnelException("İK personeli zaten aktif");

        IsActive = true;
        DeactivatedAt = null;
        DeactivationReason = null;

        // Temel yetkileri geri ver
        CanPostJobs = true;
        CanRespondToReviews = true;
        CanViewAnalytics = true;
    }

    /// <summary>
    /// İlan istatistiklerini günceller
    /// </summary>
    public void UpdateJobStats(int activeJobs, int totalApplications, int totalHires)
    {
        ActiveJobsCount = activeJobs;
        TotalApplicationsReceived = totalApplications;
        TotalHiresMade = totalHires;

        // İşe alım başarı oranını hesapla
        if (TotalApplicationsReceived > 0)
        {
            HiringSuccessRate = (decimal)TotalHiresMade / TotalApplicationsReceived * 100;
        }
    }

    /// <summary>
    /// Yeni iş ilanı yayınlandığında
    /// </summary>
    public void OnJobPosted()
    {
        TotalJobsPosted++;
        ActiveJobsCount++;
    }

    /// <summary>
    /// Yoruma yanıt verildiğinde
    /// </summary>
    public void OnReviewResponded(decimal responseTimeInHours)
    {
        TotalResponses++;

        // Ortalama yanıt süresini güncelle
        if (TotalResponses == 1)
        {
            AverageResponseTime = responseTimeInHours;
        }
        else
        {
            AverageResponseTime = ((AverageResponseTime * (TotalResponses - 1)) + responseTimeInHours) / TotalResponses;
        }

        // Güven skorunu artır (hızlı yanıt için)
        if (responseTimeInHours < 24)
        {
            IncreaseTrustScore(2);
        }
        else if (responseTimeInHours < 72)
        {
            IncreaseTrustScore(1);
        }
    }

    /// <summary>
    /// İşe alım tamamlandığında
    /// </summary>
    public void OnHiringCompleted(int daysToHire)
    {
        // Ortalama işe alım süresini güncelle
        if (TotalHiresMade == 0)
        {
            AverageTimeToHire = daysToHire;
        }
        else
        {
            AverageTimeToHire = ((AverageTimeToHire * TotalHiresMade) + daysToHire) / (TotalHiresMade + 1);
        }

        // Hızlı işe alım için güven skorunu artır
        if (daysToHire <= 14)
        {
            IncreaseTrustScore(3);
        }
        else if (daysToHire <= 30)
        {
            IncreaseTrustScore(1);
        }
    }

    /// <summary>
    /// Güven skorunu artırır
    /// </summary>
    public void IncreaseTrustScore(decimal amount)
    {
        TrustScore = Math.Min(100, TrustScore + amount);
    }

    /// <summary>
    /// Güven skorunu azaltır
    /// </summary>
    public void DecreaseTrustScore(decimal amount)
    {
        TrustScore = Math.Max(0, TrustScore - amount);
    }

    /// <summary>
    /// Yanıtlama oranını hesaplar
    /// </summary>
    public void CalculateResponseRate(int totalReviewsReceived)
    {
        if (totalReviewsReceived > 0)
        {
            ResponseRate = (decimal)TotalResponses / totalReviewsReceived * 100;
        }
    }

    /// <summary>
    /// Tam adını döndürür
    /// </summary>
    public string GetFullName()
    {
        return $"{FirstName} {LastName}";
    }

    /// <summary>
    /// Performans skorunu hesaplar
    /// </summary>
    public decimal GetPerformanceScore()
    {
        var score = 0m;

        // Güven skoru %40
        score += TrustScore * 0.4m;

        // Yanıtlama oranı %20
        score += ResponseRate * 0.2m;

        // İşe alım başarı oranı %20
        score += HiringSuccessRate * 0.2m;

        // Hız metrikleri %20
        var speedScore = 0m;
        if (AverageResponseTime < 24) speedScore = 100;
        else if (AverageResponseTime < 48) speedScore = 75;
        else if (AverageResponseTime < 72) speedScore = 50;
        else speedScore = 25;

        score += speedScore * 0.2m;

        return Math.Round(score, 2);
    }
}
