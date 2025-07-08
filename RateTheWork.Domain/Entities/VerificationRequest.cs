using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums;
using RateTheWork.Domain.Events;
using RateTheWork.Domain.Events.VerificationRequest;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Doğrulama talebi entity'si - Admin onayı bekleyen bilgi doğrulamalarını temsil eder.
/// </summary>
public class VerificationRequest : ApprovableBaseEntity
{
    // Verification Types
    public enum VerificationType
    {
        ReviewDocument,     // Yorum belgesi doğrulama
        CompanyDocument,    // Şirket belgesi doğrulama
        UserIdentity,       // Kullanıcı kimlik doğrulama
        EmploymentProof     // Çalışma belgesi doğrulama
    }

    // Document Types
    public static class DocumentTypes
    {
        public const string PaySlip = "Maaş Bordrosu";
        public const string EmploymentContract = "İş Sözleşmesi";
        public const string EmploymentCertificate = "Çalışma Belgesi";
        public const string CompanyIdCard = "Şirket Kimlik Kartı";
        public const string SeveranceLetter = "İşten Ayrılma Yazısı";
        public const string TaxReturn = "Vergi Beyannamesi";
        public const string TradeRegistry = "Ticaret Sicil Gazetesi";
        public const string Other = "Diğer";
    }

    // Properties
    public string ReviewId { get; private set; } = string.Empty;
    public string UserId { get; private set; } = string.Empty;
    public string? AdminId { get; private set; }
    public string DocumentUrl { get; private set; } = string.Empty;
    public string DocumentName { get; private set; } = string.Empty;
    public string DocumentType { get; private set; } = string.Empty;
    public VerificationType Type { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public string Status { get; private set; } = "Pending";
    public DateTime? ProcessedAt { get; private set; }
    public string? ProcessingNotes { get; private set; }
    public bool IsUrgent { get; private set; } = false;
    public int ProcessingTimeHours { get; private set; } = 0;
    public bool AllowResubmission { get; private set; } = true;
    public string? SecurityCheckNotes { get; private set; }

    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private VerificationRequest() : base()
    {
    }

    /// <summary>
    /// Yeni doğrulama talebi oluşturur (Factory method)
    /// </summary>
    public static VerificationRequest Create(
        string reviewId,
        string userId,
        string documentUrl,
        string documentName,
        string documentType,
        VerificationType verificationType = VerificationType.ReviewDocument)
    {
        ValidateDocumentUrl(documentUrl);
        ValidateDocumentName(documentName);
        ValidateDocumentType(documentType);

        var request = new VerificationRequest
        {
            ReviewId = reviewId ?? throw new ArgumentNullException(nameof(reviewId)),
            UserId = userId ?? throw new ArgumentNullException(nameof(userId)),
            DocumentUrl = documentUrl,
            DocumentName = documentName,
            DocumentType = documentType,
            Type = verificationType,
            RequestedAt = DateTime.UtcNow,
            Status = "Pending",
            IsUrgent = DetermineUrgency(documentType),
            AllowResubmission = true
        };

        // Domain Event
        request.AddDomainEvent(new VerificationRequestCreatedEvent(
            request.Id,
            userId,
            reviewId,
            documentType,
            verificationType.ToString(),
            request.RequestedAt,
            DateTime.UtcNow
        ));

        return request;
    }

    /// <summary>
    /// Talebi işleme al
    /// </summary>
    public void StartProcessing(string adminId)
    {
        if (Status != "Pending")
            throw new BusinessRuleException("Sadece beklemedeki talepler işleme alınabilir.");

        AdminId = adminId;
        Status = "Processing";
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new VerificationRequestProcessingStartedEvent(
            Id,
            adminId,
            DateTime.UtcNow,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Override Approve method from ApprovableBaseEntity
    /// </summary>
    public override void Approve(string approvedBy, string? notes = null)
    {
        if (Status != "Processing" && Status != "Pending")
            throw new BusinessRuleException("Bu durumda talep onaylanamaz.");

        base.Approve(approvedBy, notes);
        
        Status = "Approved";
        AdminId = approvedBy;
        ProcessedAt = DateTime.UtcNow;
        ProcessingNotes = notes;
        ProcessingTimeHours = (int)(ProcessedAt.Value - RequestedAt).TotalHours;

        // Domain Event
        AddDomainEvent(new VerificationRequestApprovedEvent(
            Id,
            UserId,
            ReviewId,
            approvedBy,
            DocumentType,
            ProcessingTimeHours,
            DateTime.UtcNow,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Override Reject method from ApprovableBaseEntity
    /// </summary>
    public override void Reject(string rejectedBy, string reason)
    {
        if (Status != "Processing" && Status != "Pending")
            throw new BusinessRuleException("Bu durumda talep reddedilemez.");

        base.Reject(rejectedBy, reason);
        
        Status = "Rejected";
        AdminId = rejectedBy;
        ProcessedAt = DateTime.UtcNow;
        ProcessingTimeHours = (int)(ProcessedAt.Value - RequestedAt).TotalHours;

        // Domain Event
        AddDomainEvent(new VerificationRequestRejectedEvent(
            Id,
            UserId,
            ReviewId,
            rejectedBy,
            reason,
            AllowResubmission,
            DateTime.UtcNow,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Belge yeniden yüklendi
    /// </summary>
    public void Resubmit(string newDocumentUrl, string newDocumentName)
    {
        if (Status != "Rejected" || !AllowResubmission)
            throw new BusinessRuleException("Belge yeniden gönderilemez.");

        ValidateDocumentUrl(newDocumentUrl);
        ValidateDocumentName(newDocumentName);

        DocumentUrl = newDocumentUrl;
        DocumentName = newDocumentName;
        Status = "Pending";
        AdminId = null;
        ProcessedAt = null;
        ProcessingNotes = null;
        ResetApproval();
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new VerificationRequestResubmittedEvent(
            Id,
            UserId,
            ReviewId,
            newDocumentUrl,
            DateTime.UtcNow,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Talebi acil olarak işaretle
    /// </summary>
    public void MarkAsUrgent(string markedBy, string reason)
    {
        if (IsUrgent)
            throw new BusinessRuleException("Talep zaten acil olarak işaretli.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentNullException(nameof(reason));

        IsUrgent = true;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new VerificationRequestMarkedUrgentEvent(
            Id,
            markedBy,
            reason,
            DateTime.UtcNow,
            DateTime.UtcNow
        ));
    }

    // Private helper methods
    private static bool DetermineUrgency(string documentType)
    {
        return documentType == DocumentTypes.PaySlip ||
               documentType == DocumentTypes.EmploymentContract;
    }

    // Validation methods
    private static void ValidateDocumentUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentNullException(nameof(url));

        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            throw new BusinessRuleException("Geçersiz belge URL'i.");
    }

    private static void ValidateDocumentName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        if (name.Length > 255)
            throw new BusinessRuleException("Belge adı 255 karakterden uzun olamaz.");
    }

    private static void ValidateDocumentType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentNullException(nameof(type));

        var validTypes = typeof(DocumentTypes)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Select(f => f.GetValue(null)?.ToString())
            .Where(v => v != null)
            .ToList();

        if (!validTypes.Contains(type))
            throw new BusinessRuleException("Geçersiz belge türü.");
    }
}
