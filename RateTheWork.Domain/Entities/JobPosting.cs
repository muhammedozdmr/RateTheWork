using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums.JobPosting;
using RateTheWork.Domain.Events.JobPosting;
using RateTheWork.Domain.Exceptions.JobPostingException;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// İş ilanı entity'si - Şirketlerin yayınladığı gerçek iş ilanları
/// </summary>
public class JobPosting : AuditableBaseEntity
{
    private JobPosting() : base()
    {
    }

    // Temel Bilgiler
    public string CompanyId { get; private set; } = string.Empty;
    public string HRPersonnelId { get; private set; } = string.Empty; // İK personeli ID (anonim değil!)
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Requirements { get; private set; } = string.Empty;
    public string Responsibilities { get; private set; } = string.Empty;

    // İş Detayları
    public JobType JobType { get; private set; }
    public ExperienceLevel ExperienceLevel { get; private set; }
    public string Department { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public bool IsRemoteAvailable { get; private set; }

    // Maaş Bilgileri
    public decimal? MinSalary { get; private set; }
    public decimal? MaxSalary { get; private set; }
    public string Currency { get; private set; } = "TRY";
    public bool ShowSalary { get; private set; }

    // Başvuru Süreci Bilgileri (Şeffaflık için zorunlu!)
    public DateTime ApplicationDeadline { get; private set; }
    public int TargetApplicationCount { get; private set; } // Hedeflenen başvuru sayısı
    public DateTime FirstInterviewDate { get; private set; } // İlk mülakat tarihi
    public DateTime ExpectedHiringDate { get; private set; } // İşe alım tarihi
    public string HiringProcess { get; private set; } = string.Empty; // Süreç açıklaması
    public int EstimatedProcessDays { get; private set; } // Tahmini süreç günü

    // Durum Bilgileri
    public JobPostingStatus Status { get; private set; } = JobPostingStatus.Draft;
    public DateTime? PublishedAt { get; private set; }
    public DateTime? ExpiredAt { get; private set; }
    public string? RejectionReason { get; private set; }

    // İstatistikler
    public int ViewCount { get; private set; }
    public int ApplicationCount { get; private set; }
    public int ShortlistedCount { get; private set; }
    public int InterviewedCount { get; private set; }
    public int OfferedCount { get; private set; }
    public int HiredCount { get; private set; }

    // Ek Bilgiler
    public List<string> Skills { get; private set; } = new();
    public List<string> Benefits { get; private set; } = new();
    public Dictionary<string, string> AdditionalInfo { get; private set; } = new();

    /// <summary>
    /// Yeni iş ilanı oluşturur
    /// </summary>
    public static JobPosting Create
    (
        string companyId
        , string hrPersonnelId
        , string title
        , string description
        , string requirements
        , string responsibilities
        , JobType jobType
        , ExperienceLevel experienceLevel
        , string department
        , string location
        , int targetApplicationCount
        , DateTime firstInterviewDate
        , DateTime expectedHiringDate
        , string hiringProcess
        , int estimatedProcessDays
    )
    {
        // Validasyonlar
        if (string.IsNullOrWhiteSpace(companyId))
            throw new JobPostingException("Şirket ID boş olamaz");

        if (string.IsNullOrWhiteSpace(hrPersonnelId))
            throw new JobPostingException("İK personeli ID boş olamaz");

        if (string.IsNullOrWhiteSpace(title))
            throw JobPostingException.MissingTransparencyInfo("title");

        if (title.Length < 10 || title.Length > 200)
            throw JobPostingException.InvalidJobPosting("İlan başlığı 10-200 karakter arasında olmalıdır");

        if (description.Length < 100 || description.Length > 5000)
            throw JobPostingException.InvalidJobPosting("İlan açıklaması 100-5000 karakter arasında olmalıdır");

        if (targetApplicationCount < 1)
            throw JobPostingException.InvalidJobPosting("Hedef başvuru sayısı en az 1 olmalıdır");

        if (firstInterviewDate <= DateTime.UtcNow.AddDays(3))
            throw JobPostingException.InvalidJobPosting("İlk mülakat tarihi en az 3 gün sonra olmalıdır");

        if (expectedHiringDate <= firstInterviewDate)
            throw JobPostingException.InvalidJobPosting("İşe alım tarihi mülakat tarihinden sonra olmalıdır");

        if (estimatedProcessDays < 7 || estimatedProcessDays > 90)
            throw JobPostingException.InvalidJobPosting("Tahmini süreç 7-90 gün arasında olmalıdır");

        var jobPosting = new JobPosting
        {
            Id = Guid.NewGuid().ToString(), CompanyId = companyId, HRPersonnelId = hrPersonnelId, Title = title
            , Description = description, Requirements = requirements, Responsibilities = responsibilities
            , JobType = jobType, ExperienceLevel = experienceLevel, Department = department, Location = location
            , TargetApplicationCount = targetApplicationCount, FirstInterviewDate = firstInterviewDate
            , ExpectedHiringDate = expectedHiringDate, HiringProcess = hiringProcess
            , EstimatedProcessDays = estimatedProcessDays, ApplicationDeadline = firstInterviewDate.AddDays(-3)
            , // Mülakattan 3 gün önce başvuru kapanır
            Status = JobPostingStatus.Draft
        };

        jobPosting.AddDomainEvent(new JobPostingCreatedEvent(
            jobPosting.Id,
            companyId,
            hrPersonnelId,
            title
        ));

        return jobPosting;
    }

    /// <summary>
    /// Maaş bilgilerini ekler
    /// </summary>
    public void SetSalaryInfo(decimal? minSalary, decimal? maxSalary, string currency = "TRY", bool showSalary = true)
    {
        if (minSalary.HasValue && maxSalary.HasValue && minSalary > maxSalary)
            throw JobPostingException.InvalidJobPosting("Minimum maaş maksimum maaştan büyük olamaz");

        MinSalary = minSalary;
        MaxSalary = maxSalary;
        Currency = currency;
        ShowSalary = showSalary;
    }

    /// <summary>
    /// Yetenekleri ekler
    /// </summary>
    public void SetSkills(List<string> skills)
    {
        if (skills == null || !skills.Any())
            throw JobPostingException.InvalidJobPosting("En az bir yetenek eklenmeli");

        Skills = skills.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
    }

    /// <summary>
    /// Yan hakları ekler
    /// </summary>
    public void SetBenefits(List<string> benefits)
    {
        Benefits = benefits?.Where(b => !string.IsNullOrWhiteSpace(b)).Distinct().ToList() ?? new List<string>();
    }

    /// <summary>
    /// İlanı yayınlar
    /// </summary>
    public void Publish()
    {
        if (Status != JobPostingStatus.Draft && Status != JobPostingStatus.PendingApproval)
            throw JobPostingException.InvalidStatus(Id, Status.ToString(), "Active");

        Status = JobPostingStatus.Active;
        PublishedAt = DateTime.UtcNow;
        ExpiredAt = ApplicationDeadline;

        AddDomainEvent(new JobPostingPublishedEvent(
            Id,
            CompanyId,
            Title,
            PublishedAt.Value
        ));
    }

    /// <summary>
    /// İlanı durdurur
    /// </summary>
    public void Pause()
    {
        if (Status != JobPostingStatus.Active)
            throw JobPostingException.InvalidStatus(Id, Status.ToString(), "Paused");

        Status = JobPostingStatus.Paused;
    }

    /// <summary>
    /// İlanı yeniden aktif eder
    /// </summary>
    public void Resume()
    {
        if (Status != JobPostingStatus.Paused)
            throw JobPostingException.InvalidStatus(Id, Status.ToString(), "Active");

        if (ApplicationDeadline < DateTime.UtcNow)
            throw JobPostingException.AlreadyExpired(Id);

        Status = JobPostingStatus.Active;
    }

    /// <summary>
    /// İlanı tamamlar
    /// </summary>
    public void Complete(int actualHiredCount)
    {
        if (Status != JobPostingStatus.Active && Status != JobPostingStatus.Paused)
            throw JobPostingException.InvalidStatus(Id, Status.ToString(), "Completed");

        Status = JobPostingStatus.Completed;
        HiredCount = actualHiredCount;

        AddDomainEvent(new JobPostingCompletedEvent(
            Id,
            CompanyId,
            Title,
            ApplicationCount,
            HiredCount,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// İlanı iptal eder
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status == JobPostingStatus.Completed || Status == JobPostingStatus.Cancelled)
            throw JobPostingException.InvalidStatus(Id, Status.ToString(), "Cancelled");

        Status = JobPostingStatus.Cancelled;
        RejectionReason = reason;
    }

    /// <summary>
    /// İlanı reddeder (Admin)
    /// </summary>
    public void Reject(string reason)
    {
        if (Status != JobPostingStatus.PendingApproval)
            throw JobPostingException.InvalidStatus(Id, Status.ToString(), "Rejected");

        Status = JobPostingStatus.Rejected;
        RejectionReason = reason;
    }

    /// <summary>
    /// Görüntülenme sayısını artırır
    /// </summary>
    public void IncrementViewCount()
    {
        ViewCount++;
    }

    /// <summary>
    /// Başvuru sayısını artırır
    /// </summary>
    public void IncrementApplicationCount()
    {
        ApplicationCount++;

        // Hedef başvuru sayısına ulaşıldıysa uyarı eventi
        if (ApplicationCount >= TargetApplicationCount)
        {
            AddDomainEvent(new JobPostingTargetReachedEvent(
                Id,
                CompanyId,
                Title,
                TargetApplicationCount
            ));
        }
    }

    /// <summary>
    /// İşe alım süreci istatistiklerini günceller
    /// </summary>
    public void UpdateProcessStats(int shortlisted, int interviewed, int offered)
    {
        ShortlistedCount = shortlisted;
        InterviewedCount = interviewed;
        OfferedCount = offered;
    }

    /// <summary>
    /// İlanın süresi dolmuş mu kontrol eder
    /// </summary>
    public bool IsExpired()
    {
        return ApplicationDeadline < DateTime.UtcNow ||
               (ExpiredAt.HasValue && ExpiredAt.Value < DateTime.UtcNow);
    }

    /// <summary>
    /// İlanın havuz ilanı olup olmadığını kontrol eder
    /// </summary>
    public bool IsSuspiciousPoolJob()
    {
        // Çok yüksek hedef başvuru sayısı
        if (TargetApplicationCount > 500)
            return true;

        // İşe alım süreci çok uzun
        if (EstimatedProcessDays > 60)
            return true;

        // Başvuru sayısı hedefe ulaştı ama işe alım yok
        if (ApplicationCount >= TargetApplicationCount &&
            HiredCount == 0 &&
            (DateTime.UtcNow - PublishedAt).Value.TotalDays > 30)
            return true;

        return false;
    }
}
