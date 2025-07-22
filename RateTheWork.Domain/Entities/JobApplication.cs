using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums.JobApplication;
using RateTheWork.Domain.Events.JobApplication;
using RateTheWork.Domain.Exceptions.JobApplicationException;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// İş başvurusu entity'si
/// </summary>
public class JobApplication : AuditableBaseEntity
{
    private JobApplication() : base()
    {
    }

    // Temel Bilgiler
    public string JobPostingId { get; private set; } = string.Empty;
    public string ApplicantUserId { get; private set; } = string.Empty;
    public string CompanyId { get; private set; } = string.Empty;
    public ApplicationStatus Status { get; private set; } = ApplicationStatus.Received;

    // Başvuru Detayları
    public string CoverLetter { get; private set; } = string.Empty;
    public string ResumeUrl { get; private set; } = string.Empty;
    public string? PortfolioUrl { get; private set; }
    public string? LinkedInProfile { get; private set; }
    public List<string> AdditionalDocuments { get; private set; } = new();

    // İletişim Bilgileri
    public string ApplicantName { get; private set; } = string.Empty;
    public string ApplicantEmail { get; private set; } = string.Empty;
    public string ApplicantPhone { get; private set; } = string.Empty;

    // Süreç Bilgileri
    public DateTime? ReviewedAt { get; private set; }
    public string? ReviewedBy { get; private set; } // HR Personnel ID
    public DateTime? InterviewDate { get; private set; }
    public string? InterviewNotes { get; private set; }
    public int? InterviewScore { get; private set; } // 1-10
    public DateTime? OfferDate { get; private set; }
    public decimal? OfferedSalary { get; private set; }
    public DateTime? ResponseDeadline { get; private set; }

    // Durum Geçmişi
    public List<ApplicationStatusHistory> StatusHistory { get; private set; } = new();

    // Notlar ve Değerlendirmeler
    public string? InternalNotes { get; private set; }
    public Dictionary<string, int> SkillAssessments { get; private set; } = new(); // Skill -> Score
    public bool IsFavorite { get; private set; }
    public string? RejectionReason { get; private set; }

    /// <summary>
    /// Yeni iş başvurusu oluşturur
    /// </summary>
    public static JobApplication Create
    (
        string jobPostingId
        , string applicantUserId
        , string companyId
        , string applicantName
        , string applicantEmail
        , string applicantPhone
        , string coverLetter
        , string resumeUrl
    )
    {
        // Validasyonlar
        if (string.IsNullOrWhiteSpace(jobPostingId))
            throw new JobApplicationException("İş ilanı ID boş olamaz");

        if (string.IsNullOrWhiteSpace(applicantUserId))
            throw new JobApplicationException("Başvuran kullanıcı ID boş olamaz");

        if (string.IsNullOrWhiteSpace(applicantName))
            throw new JobApplicationException("Başvuran adı boş olamaz");

        if (string.IsNullOrWhiteSpace(applicantEmail) || !applicantEmail.Contains("@"))
            throw new JobApplicationException("Geçerli bir email adresi giriniz");

        if (string.IsNullOrWhiteSpace(coverLetter) || coverLetter.Length < 50)
            throw new JobApplicationException("Ön yazı en az 50 karakter olmalıdır");

        if (string.IsNullOrWhiteSpace(resumeUrl))
            throw new JobApplicationException("Özgeçmiş zorunludur");

        var application = new JobApplication
        {
            Id = Guid.NewGuid().ToString(), JobPostingId = jobPostingId, ApplicantUserId = applicantUserId
            , CompanyId = companyId, ApplicantName = applicantName.Trim()
            , ApplicantEmail = applicantEmail.ToLowerInvariant().Trim(), ApplicantPhone = applicantPhone.Trim()
            , CoverLetter = coverLetter.Trim(), ResumeUrl = resumeUrl, Status = ApplicationStatus.Received
        };

        // İlk durum geçmişini ekle
        application.AddStatusHistory(ApplicationStatus.Received, "Başvuru alındı");

        application.AddDomainEvent(new JobApplicationCreatedEvent(
            application.Id,
            jobPostingId,
            applicantUserId,
            applicantName
        ));

        return application;
    }

    /// <summary>
    /// Ek belgeleri ekler
    /// </summary>
    public void AddAdditionalDocuments(List<string> documentUrls)
    {
        if (documentUrls == null || !documentUrls.Any())
            return;

        AdditionalDocuments.AddRange(documentUrls.Where(d => !string.IsNullOrWhiteSpace(d)));
    }

    /// <summary>
    /// Profil bilgilerini ekler
    /// </summary>
    public void SetProfileLinks(string? portfolioUrl, string? linkedInProfile)
    {
        PortfolioUrl = portfolioUrl?.Trim();
        LinkedInProfile = linkedInProfile?.Trim();
    }

    /// <summary>
    /// Başvuruyu inceler
    /// </summary>
    public void Review(string hrPersonnelId, string? notes = null)
    {
        if (Status != ApplicationStatus.Received)
            throw JobApplicationException.InvalidStatus(Id, Status.ToString(), "UnderReview");

        Status = ApplicationStatus.UnderReview;
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = hrPersonnelId;
        InternalNotes = notes;

        AddStatusHistory(ApplicationStatus.UnderReview, "Başvuru incelemeye alındı", hrPersonnelId);
    }

    /// <summary>
    /// Başvuruyu ön elemeye alır
    /// </summary>
    public void Shortlist(string hrPersonnelId, string? notes = null)
    {
        if (Status != ApplicationStatus.UnderReview)
            throw JobApplicationException.InvalidStatus(Id, Status.ToString(), "Shortlisted");

        Status = ApplicationStatus.Shortlisted;
        if (!string.IsNullOrWhiteSpace(notes))
            InternalNotes = $"{InternalNotes}\n{notes}";

        AddStatusHistory(ApplicationStatus.Shortlisted, "Ön elemeye alındı", hrPersonnelId);
    }

    /// <summary>
    /// Mülakata davet eder
    /// </summary>
    public void InviteToInterview(string hrPersonnelId, DateTime interviewDate)
    {
        if (Status != ApplicationStatus.Shortlisted)
            throw JobApplicationException.InvalidStatus(Id, Status.ToString(), "InterviewInvited");

        if (interviewDate <= DateTime.UtcNow)
            throw JobApplicationException.InterviewDateInPast(Id, interviewDate);

        Status = ApplicationStatus.InterviewInvited;
        InterviewDate = interviewDate;

        AddStatusHistory(ApplicationStatus.InterviewInvited,
            $"Mülakat daveti gönderildi. Tarih: {interviewDate:dd.MM.yyyy HH:mm}", hrPersonnelId);

        AddDomainEvent(new ApplicantInvitedToInterviewEvent(
            Id,
            JobPostingId,
            ApplicantUserId,
            interviewDate
        ));
    }

    /// <summary>
    /// Mülakat sonucunu kaydeder
    /// </summary>
    public void RecordInterview(string hrPersonnelId, int score, string notes)
    {
        if (Status != ApplicationStatus.InterviewInvited && Status != ApplicationStatus.Interviewed)
            throw JobApplicationException.InvalidStatus(Id, Status.ToString(), "Interviewed");

        if (score < 1 || score > 10)
            throw new JobApplicationException("Mülakat skoru 1-10 arasında olmalıdır");

        Status = ApplicationStatus.Interviewed;
        InterviewScore = score;
        InterviewNotes = notes;

        AddStatusHistory(ApplicationStatus.Interviewed,
            $"Mülakat tamamlandı. Skor: {score}/10", hrPersonnelId);
    }

    /// <summary>
    /// İş teklifi yapar
    /// </summary>
    public void MakeOffer(string hrPersonnelId, decimal salary, DateTime responseDeadline)
    {
        if (Status != ApplicationStatus.Interviewed && Status != ApplicationStatus.UnderEvaluation)
            throw JobApplicationException.InvalidStatus(Id, Status.ToString(), "OfferMade");

        if (salary <= 0)
            throw JobApplicationException.InvalidSalaryOffer(Id, salary);

        if (responseDeadline <= DateTime.UtcNow)
            throw new JobApplicationException("Yanıt süresi gelecek bir tarih olmalıdır");

        Status = ApplicationStatus.OfferMade;
        OfferDate = DateTime.UtcNow;
        OfferedSalary = salary;
        ResponseDeadline = responseDeadline;

        AddStatusHistory(ApplicationStatus.OfferMade,
            $"İş teklifi yapıldı. Maaş: {salary:C}", hrPersonnelId);

        AddDomainEvent(new JobOfferMadeEvent(
            Id,
            JobPostingId,
            ApplicantUserId,
            salary
        ));
    }

    /// <summary>
    /// Teklif kabul edildi
    /// </summary>
    public void AcceptOffer()
    {
        if (Status != ApplicationStatus.OfferMade)
            throw JobApplicationException.InvalidStatus(Id, Status.ToString(), "OfferAccepted");

        Status = ApplicationStatus.OfferAccepted;
        AddStatusHistory(ApplicationStatus.OfferAccepted, "Teklif kabul edildi");
    }

    /// <summary>
    /// Teklif reddedildi
    /// </summary>
    public void DeclineOffer(string? reason = null)
    {
        if (Status != ApplicationStatus.OfferMade)
            throw JobApplicationException.InvalidStatus(Id, Status.ToString(), "OfferDeclined");

        Status = ApplicationStatus.OfferDeclined;
        AddStatusHistory(ApplicationStatus.OfferDeclined,
            $"Teklif reddedildi{(string.IsNullOrWhiteSpace(reason) ? "" : $": {reason}")}");
    }

    /// <summary>
    /// İşe alım tamamlandı
    /// </summary>
    public void CompleteHiring(string hrPersonnelId)
    {
        if (Status != ApplicationStatus.OfferAccepted)
            throw JobApplicationException.InvalidStatus(Id, Status.ToString(), "Hired");

        Status = ApplicationStatus.Hired;
        AddStatusHistory(ApplicationStatus.Hired, "İşe alım tamamlandı", hrPersonnelId);

        AddDomainEvent(new ApplicantHiredEvent(
            Id,
            JobPostingId,
            ApplicantUserId,
            CompanyId
        ));
    }

    /// <summary>
    /// Başvuruyu reddeder
    /// </summary>
    public void Reject(string hrPersonnelId, string reason)
    {
        if (Status == ApplicationStatus.Hired || Status == ApplicationStatus.Rejected)
            throw JobApplicationException.AlreadyProcessed(Id, Status.ToString());

        Status = ApplicationStatus.Rejected;
        RejectionReason = reason;
        AddStatusHistory(ApplicationStatus.Rejected, $"Başvuru reddedildi: {reason}", hrPersonnelId);
    }

    /// <summary>
    /// Aday başvurusunu geri çeker
    /// </summary>
    public void Withdraw(string reason)
    {
        if (Status == ApplicationStatus.Hired || Status == ApplicationStatus.Rejected)
            throw JobApplicationException.WithdrawnApplication(Id);

        Status = ApplicationStatus.Withdrawn;
        AddStatusHistory(ApplicationStatus.Withdrawn, $"Aday başvurusunu geri çekti: {reason}");
    }

    /// <summary>
    /// Yetenek havuzuna alır
    /// </summary>
    public void MoveToTalentPool(string hrPersonnelId, string notes)
    {
        if (Status == ApplicationStatus.Hired || Status == ApplicationStatus.TalentPool)
            throw JobApplicationException.AlreadyProcessed(Id, Status.ToString());

        Status = ApplicationStatus.TalentPool;
        AddStatusHistory(ApplicationStatus.TalentPool,
            $"Yetenek havuzuna alındı: {notes}", hrPersonnelId);
    }

    /// <summary>
    /// Favori olarak işaretler
    /// </summary>
    public void ToggleFavorite()
    {
        IsFavorite = !IsFavorite;
    }

    /// <summary>
    /// Yetenek değerlendirmesi ekler
    /// </summary>
    public void AssessSkill(string skill, int score)
    {
        if (score < 1 || score > 10)
            throw new JobApplicationException("Yetenek skoru 1-10 arasında olmalıdır");

        SkillAssessments[skill] = score;
    }

    /// <summary>
    /// Dahili not ekler
    /// </summary>
    public void AddInternalNote(string note, string hrPersonnelId)
    {
        var timestamp = DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm");
        InternalNotes = $"{InternalNotes}\n[{timestamp} - {hrPersonnelId}] {note}";
    }

    /// <summary>
    /// Durum geçmişi ekler
    /// </summary>
    private void AddStatusHistory(ApplicationStatus status, string notes, string? changedBy = null)
    {
        StatusHistory.Add(new ApplicationStatusHistory
        {
            Status = status, ChangedAt = DateTime.UtcNow, ChangedBy = changedBy, Notes = notes
        });
    }

    /// <summary>
    /// Başvuru sürecinin ne kadar sürdüğünü hesaplar
    /// </summary>
    public int GetProcessDurationInDays()
    {
        if (Status == ApplicationStatus.Hired || Status == ApplicationStatus.Rejected)
        {
            var lastStatus = StatusHistory.LastOrDefault();
            if (lastStatus != null)
            {
                return (lastStatus.ChangedAt - CreatedAt).Days;
            }
        }

        return (DateTime.UtcNow - CreatedAt).Days;
    }
}

/// <summary>
/// Başvuru durum geçmişi
/// </summary>
public class ApplicationStatusHistory
{
    public ApplicationStatus Status { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
    public string Notes { get; set; } = string.Empty;
}
