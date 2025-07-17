using System.Reflection;
using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums.Verification;
using RateTheWork.Domain.Enums.VerificationRequest;
using RateTheWork.Domain.Events.VerificationRequest;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Doğrulama talebi entity'si - Admin onayı bekleyen bilgi doğrulamalarını temsil eder.
/// </summary>
public class VerificationRequest : ApprovableBaseEntity
{
    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private VerificationRequest() : base()
    {
    }

    // Properties
    public string ReviewId { get; private set; } = string.Empty;
    public string UserId { get; private set; } = string.Empty;
    public string CompanyId { get; private set; } = string.Empty;
    public string? AdminId { get; private set; }
    public string DocumentUrl { get; private set; } = string.Empty;
    public string DocumentName { get; private set; } = string.Empty;
    public string DocumentType { get; private set; } = string.Empty;
    public VerificationType Type { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public VerificationRequestStatus Status { get; private set; } = VerificationRequestStatus.Pending;
    public DateTime? ProcessedAt { get; private set; }
    public string? ProcessingNotes { get; private set; }
    public bool IsUrgent { get; private set; } = false;
    public int ProcessingTimeHours { get; private set; } = 0;
    public bool AllowResubmission { get; private set; } = true;
    public string? SecurityCheckNotes { get; private set; }

    /// <summary>
    /// Yeni doğrulama talebi oluşturur (Factory method)
    /// </summary>
    public static VerificationRequest Create
    (
        string reviewId
        , string userId
        , string companyId
        , string documentUrl
        , string documentName
        , string documentType
        , VerificationType verificationType = VerificationType.ReviewDocument
    )
    {
        ValidateDocumentUrl(documentUrl);
        ValidateDocumentName(documentName);
        ValidateDocumentType(documentType);

        var request = new VerificationRequest
        {
            ReviewId = reviewId ?? throw new ArgumentNullException(nameof(reviewId))
            , UserId = userId ?? throw new ArgumentNullException(nameof(userId))
            , CompanyId = companyId ?? throw new ArgumentNullException(nameof(companyId))
            , DocumentUrl = documentUrl
            , DocumentName = documentName, DocumentType = documentType, Type = verificationType
            , RequestedAt = DateTime.UtcNow, Status = VerificationRequestStatus.Pending
            , IsUrgent = DetermineUrgency(documentType)
            , AllowResubmission = true
        };

        // Domain Event
        request.AddDomainEvent(new VerificationRequestCreatedEvent(
            request.Id,
            userId,
            reviewId,
            documentType,
            verificationType.ToString(),
            request.RequestedAt
        ));

        return request;
    }

    /// <summary>
    /// Yorum belgesi doğrulama talebi oluşturur
    /// </summary>
    public static VerificationRequest CreateForReviewDocument
    (
        string reviewId
        , string userId
        , string companyId
        , string documentUrl
        , string documentName
        , string documentType
    )
    {
        return Create(
            reviewId,
            userId,
            companyId,
            documentUrl,
            documentName,
            documentType,
            VerificationType.ReviewDocument
        );
    }

    /// <summary>
    /// Şirket belgesi doğrulama talebi oluşturur
    /// </summary>
    public static VerificationRequest CreateForCompanyDocument
    (
        string companyId
        , string userId
        , string documentUrl
        , string documentName
        , string documentType
    )
    {
        return Create(
            string.Empty, // reviewId boş - şirket belgesi için review yok
            userId,
            companyId,
            documentUrl,
            documentName,
            documentType,
            VerificationType.CompanyDocument
        );
    }

    /// <summary>
    /// Kullanıcı kimlik doğrulama talebi oluşturur
    /// </summary>
    public static VerificationRequest CreateForUserIdentity
    (
        string userId
        , string documentUrl
        , string documentName
    )
    {
        return Create(
            string.Empty, // reviewId boş - kimlik doğrulama için
            userId,
            string.Empty, // companyId boş - kimlik doğrulama için
            documentUrl,
            documentName,
            "Kimlik Belgesi",
            VerificationType.UserIdentity
        );
    }

    /// <summary>
    /// Çalışma belgesi doğrulama talebi oluşturur
    /// </summary>
    public static VerificationRequest CreateForEmploymentProof
    (
        string reviewId
        , string userId
        , string companyId
        , string documentUrl
        , string documentName
        , string documentType
    )
    {
        var request = Create(
            reviewId,
            userId,
            companyId,
            documentUrl,
            documentName,
            documentType,
            VerificationType.EmploymentProof
        );

        // Çalışma belgeleri genellikle acildir
        request.IsUrgent = true;

        return request;
    }

    /// <summary>
    /// Acil doğrulama talebi oluşturur
    /// </summary>
    public static VerificationRequest CreateUrgent
    (
        string reviewId
        , string userId
        , string companyId
        , string documentUrl
        , string documentName
        , string documentType
        , VerificationType verificationType
        , string urgencyReason
    )
    {
        var request = Create(
            reviewId,
            userId,
            companyId,
            documentUrl,
            documentName,
            documentType,
            verificationType
        );

        request.IsUrgent = true;
        request.ProcessingNotes = $"ACİL: {urgencyReason}";

        // Acil talep için özel event
        request.AddDomainEvent(new VerificationRequestMarkedUrgentEvent(
            request.Id,
            "SYSTEM",
            urgencyReason,
            DateTime.UtcNow
        ));

        return request;
    }

    /// <summary>
    /// Talebi işleme al
    /// </summary>
    public void StartProcessing(string adminId)
    {
        if (Status != VerificationRequestStatus.Pending)
            throw new BusinessRuleException("Sadece beklemedeki talepler işleme alınabilir.");

        AdminId = adminId;
        Status = VerificationRequestStatus.Pending; // Keep as pending during processing
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new VerificationRequestProcessingStartedEvent(
            Id,
            adminId,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Override Approve method from ApprovableBaseEntity
    /// </summary>
    public override void Approve(string approvedBy, string? notes = null)
    {
        if (Status != VerificationRequestStatus.Pending)
            throw new BusinessRuleException("Bu durumda talep onaylanamaz.");

        base.Approve(approvedBy, notes);

        Status = VerificationRequestStatus.Approved;
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
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Override Reject method from ApprovableBaseEntity
    /// </summary>
    public override void Reject(string rejectedBy, string reason)
    {
        if (Status != VerificationRequestStatus.Pending)
            throw new BusinessRuleException("Bu durumda talep reddedilemez.");

        base.Reject(rejectedBy, reason);

        Status = VerificationRequestStatus.Rejected;
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
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Belge yeniden yüklendi
    /// </summary>
    public void Resubmit(string newDocumentUrl, string newDocumentName)
    {
        if (Status != VerificationRequestStatus.Rejected || !AllowResubmission)
            throw new BusinessRuleException("Belge yeniden gönderilemez.");

        ValidateDocumentUrl(newDocumentUrl);
        ValidateDocumentName(newDocumentName);

        DocumentUrl = newDocumentUrl;
        DocumentName = newDocumentName;
        Status = VerificationRequestStatus.Pending;
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
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(f => f.GetValue(null)?.ToString())
            .Where(v => v != null)
            .ToList();

        if (!validTypes.Contains(type))
            throw new BusinessRuleException("Geçersiz belge türü.");
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
}
