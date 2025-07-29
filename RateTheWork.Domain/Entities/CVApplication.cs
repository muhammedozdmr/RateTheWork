using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums.CVApplication;
using RateTheWork.Domain.Events.CVApplication;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.Common;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// CV başvurusu entity'si - Kullanıcıların şirketlere yaptığı CV başvurularını temsil eder
/// </summary>
public class CVApplication : AuditableBaseEntity, IAggregateRoot
{
    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private CVApplication() : base()
    {
    }

    // Properties
    public string UserId { get; private set; } = string.Empty;
    public string CompanyId { get; private set; } = string.Empty;
    public List<string> DepartmentIds { get; private set; } = new List<string>(); // Max 3 departman
    public string ApplicantName { get; private set; } = string.Empty;
    public string ApplicantEmail { get; private set; } = string.Empty;
    public string ApplicantPhone { get; private set; } = string.Empty;
    public string? ApplicantWebsite { get; private set; }
    public string CVFileUrl { get; private set; } = string.Empty;
    public string? MotivationLetterUrl { get; private set; }
    public CVApplicationStatus Status { get; private set; } = CVApplicationStatus.Pending;
    public DateTime SubmittedAt { get; private set; }
    public DateTime? ViewedAt { get; private set; }
    public DateTime? DownloadedAt { get; private set; }
    public DateTime? RespondedAt { get; private set; }
    public string? ResponseMessage { get; private set; }
    public DateTime ExpiryDate { get; private set; } // 90 gün sonra silinir
    public DateTime? FeedbackDeadline { get; private set; } // İndirildikten 30 gün sonra
    public new bool IsDeleted { get; private set; } = false;
    public string? DeleteReason { get; private set; }
    public Dictionary<string, string> AdditionalInfo { get; private set; } = new Dictionary<string, string>();
    
    // Navigation
    public virtual User? User { get; private set; }
    public virtual Company? Company { get; private set; }
    public virtual ICollection<Department> Departments { get; private set; } = new List<Department>();

    /// <summary>
    /// Yeni CV başvurusu oluşturur
    /// </summary>
    public static CVApplication Create(
        string userId,
        string companyId,
        List<string> departmentIds,
        string applicantName,
        string applicantEmail,
        string applicantPhone,
        string cvFileUrl,
        string? applicantWebsite = null,
        string? motivationLetterUrl = null,
        Dictionary<string, string>? additionalInfo = null)
    {
        // Validations
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));
            
        if (string.IsNullOrWhiteSpace(companyId))
            throw new ArgumentNullException(nameof(companyId));
            
        if (departmentIds == null || departmentIds.Count == 0)
            throw new BusinessRuleException("En az bir departman seçilmelidir.");
            
        if (departmentIds.Count > 3)
            throw new BusinessRuleException("En fazla 3 departmana başvuru yapılabilir.");
            
        if (string.IsNullOrWhiteSpace(applicantName))
            throw new ArgumentNullException(nameof(applicantName));
            
        if (string.IsNullOrWhiteSpace(applicantEmail))
            throw new ArgumentNullException(nameof(applicantEmail));
            
        if (string.IsNullOrWhiteSpace(applicantPhone))
            throw new ArgumentNullException(nameof(applicantPhone));
            
        if (string.IsNullOrWhiteSpace(cvFileUrl))
            throw new ArgumentNullException(nameof(cvFileUrl));

        var cvApplication = new CVApplication
        {
            UserId = userId,
            CompanyId = companyId,
            DepartmentIds = departmentIds.Distinct().ToList(),
            ApplicantName = applicantName.Trim(),
            ApplicantEmail = applicantEmail.ToLowerInvariant(),
            ApplicantPhone = applicantPhone.Trim(),
            ApplicantWebsite = applicantWebsite?.Trim(),
            CVFileUrl = cvFileUrl,
            MotivationLetterUrl = motivationLetterUrl,
            Status = CVApplicationStatus.Pending,
            SubmittedAt = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(90),
            AdditionalInfo = additionalInfo ?? new Dictionary<string, string>()
        };

        // Domain Event
        cvApplication.AddDomainEvent(new CVApplicationSubmittedEvent(
            cvApplication.Id,
            cvApplication.UserId,
            cvApplication.CompanyId,
            cvApplication.DepartmentIds,
            cvApplication.ApplicantName,
            cvApplication.SubmittedAt
        ));

        return cvApplication;
    }

    /// <summary>
    /// CV görüntülendi olarak işaretle
    /// </summary>
    public void MarkAsViewed(string viewedBy)
    {
        if (ViewedAt.HasValue)
            return; // Zaten görüntülendi

        ViewedAt = DateTime.UtcNow;
        Status = CVApplicationStatus.Viewed;
        SetModifiedDate();

        AddDomainEvent(new CVApplicationViewedEvent(
            Id,
            CompanyId,
            viewedBy,
            ViewedAt.Value
        ));
    }

    /// <summary>
    /// CV indirildi olarak işaretle
    /// </summary>
    public void MarkAsDownloaded(string downloadedBy)
    {
        if (!ViewedAt.HasValue)
            MarkAsViewed(downloadedBy); // Önce görüntülendi olarak işaretle

        DownloadedAt = DateTime.UtcNow;
        FeedbackDeadline = DateTime.UtcNow.AddDays(30); // 30 gün geri bildirim süresi
        Status = CVApplicationStatus.Downloaded;
        SetModifiedDate();

        AddDomainEvent(new CVApplicationDownloadedEvent(
            Id,
            CompanyId,
            downloadedBy,
            DownloadedAt.Value,
            FeedbackDeadline.Value
        ));
    }

    /// <summary>
    /// Şirket tarafından yanıt verildi
    /// </summary>
    public void Respond(string responseMessage, CVApplicationStatus status)
    {
        if (RespondedAt.HasValue)
            throw new BusinessRuleException("Bu başvuruya zaten yanıt verilmiş.");

        if (string.IsNullOrWhiteSpace(responseMessage))
            throw new ArgumentNullException(nameof(responseMessage));

        var validStatuses = new[] { 
            CVApplicationStatus.Accepted, 
            CVApplicationStatus.Rejected, 
            CVApplicationStatus.OnHold 
        };

        if (!validStatuses.Contains(status))
            throw new BusinessRuleException("Geçersiz yanıt durumu.");

        RespondedAt = DateTime.UtcNow;
        ResponseMessage = responseMessage.Trim();
        Status = status;
        SetModifiedDate();

        AddDomainEvent(new CVApplicationRespondedEvent(
            Id,
            CompanyId,
            UserId,
            Status,
            ResponseMessage,
            RespondedAt.Value
        ));
    }

    /// <summary>
    /// CV başvurusunu sil (90 gün sonra otomatik veya manuel)
    /// </summary>
    public void Delete(string reason)
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeleteReason = reason;
        SetModifiedDate();

        AddDomainEvent(new CVApplicationDeletedEvent(
            Id,
            CompanyId,
            UserId,
            reason,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Geri bildirim süresi doldu mu kontrol et
    /// </summary>
    public bool IsFeedbackOverdue()
    {
        return FeedbackDeadline.HasValue && 
               DateTime.UtcNow > FeedbackDeadline.Value && 
               !RespondedAt.HasValue;
    }

    /// <summary>
    /// CV süresi doldu mu kontrol et
    /// </summary>
    public bool IsExpired()
    {
        return DateTime.UtcNow > ExpiryDate;
    }
}