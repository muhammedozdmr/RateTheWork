using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums;
using RateTheWork.Domain.Events;
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
        CompanyDocument,   // Şirket belgesi doğrulama
        UserIdentity,      // Kullanıcı kimlik doğrulama
        EmploymentProof    // Çalışma belgesi doğrulama
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
    public string ReviewId { get; private set; }
    public string UserId { get; private set; }
    public string? AdminId { get; private set; }
    public string DocumentUrl { get; private set; }
    public string DocumentName { get; private set; }
    public string DocumentType { get; private set; }
    public VerificationType Type { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public string Status { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? ProcessingNotes { get; private set; }
    public bool IsUrgent { get; private set; }
    public int ProcessingTimeHours { get; private set; } // İşlem süresi (saat)
    public string? RejectionReason { get; private set; }
    public bool AllowResubmission { get; private set; } // Tekrar gönderilebilir mi?
    public string? SecurityCheckNotes { get; private set; } // Güvenlik kontrol notları

    /// <summary>
    /// EF Core için private constructor
    /// </summary>
    private VerificationRequest() : base() { }

    /// <summary>
    /// Yeni doğrulama talebi oluşturur
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
            Status = VerificationStatuses.Pending,
            IsUrgent = false,
            ProcessingTimeHours = 0,
            AllowResubmission = true
        };

        // Belge tipine göre aciliyet belirleme
        if (documentType == DocumentTypes.EmploymentContract || 
            documentType == DocumentTypes.PaySlip)
        {
            request.IsUrgent = true;
        }

        // Domain Event
        request.AddDomainEvent(new VerificationRequestCreatedEvent(
            request.Id,
            userId,
            reviewId,
            documentType
        ));

        return request;
    }

    /// <summary>
    /// Talebi işleme al
    /// </summary>
    public void StartProcessing(string adminId)
    {
        if (Status != VerificationStatuses.Pending)
            throw new BusinessRuleException($"Sadece '{VerificationStatuses.Pending}' durumundaki talepler işleme alınabilir.");

        AdminId = adminId;
        Status = "Processing";
        SetModifiedDate();

        AddDomainEvent(new VerificationRequestProcessingStartedEvent(Id, adminId));
    }

    /// <summary>
    /// Doğrulama talebini onayla
    /// </summary>
    public override void Approve(string approvedBy, string? notes = null)
    {
        if (Status != "Processing" && Status != VerificationStatuses.Pending)
            throw new BusinessRuleException("Bu durumda talep onaylanamaz.");

        // Temel onaylama işlemi
        base.Approve(approvedBy, notes);
        
        // VerificationRequest'e özel işlemler
        Status = VerificationStatuses.Approved;
        AdminId = approvedBy;
        ProcessedAt = DateTime.UtcNow;
        ProcessingNotes = notes;
        
        // İşlem süresini hesapla
        ProcessingTimeHours = (int)(ProcessedAt.Value - RequestedAt).TotalHours;

        // Domain Event
        AddDomainEvent(new VerificationRequestApprovedEvent(
            Id,
            UserId,
            ReviewId,
            approvedBy,
            DocumentType
        ));
    }

    /// <summary>
    /// Doğrulama talebini reddet
    /// </summary>
    public override void Reject(string rejectedBy, string reason)
    {
        if (Status != "Processing" && Status != VerificationStatuses.Pending)
            throw new BusinessRuleException("Bu durumda talep reddedilemez.");

        // Temel reddetme işlemi
        base.Reject(rejectedBy, reason);
        
        // VerificationRequest'e özel işlemler
        Status = VerificationStatuses.Rejected;
        AdminId = rejectedBy;
        ProcessedAt = DateTime.UtcNow;
        RejectionReason = reason;
        
        // İşlem süresini hesapla
        ProcessingTimeHours = (int)(ProcessedAt.Value - RequestedAt).TotalHours;

        // Domain Event
        AddDomainEvent(new VerificationRequestRejectedEvent(
            Id,
            UserId,
            ReviewId,
            rejectedBy,
            reason
        ));
    }

    /// <summary>
    /// Güvenlik kontrolü ekle
    /// </summary>
    public void AddSecurityCheck(string adminId, string checkNotes, bool passed)
    {
        if (Status != "Processing")
            throw new BusinessRuleException("Güvenlik kontrolü sadece işlemdeki taleplere eklenebilir.");

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
        var checkResult = passed ? "GEÇTİ" : "BAŞARISIZ";
        var note = $"[{timestamp}] Güvenlik Kontrolü ({adminId}): {checkResult} - {checkNotes}";

        SecurityCheckNotes = string.IsNullOrWhiteSpace(SecurityCheckNotes)
            ? note
            : $"{SecurityCheckNotes}\n{note}";

        if (!passed)
        {
            // Güvenlik kontrolünden geçemezse otomatik red
            Reject(adminId, $"Güvenlik kontrolü başarısız: {checkNotes}");
            AllowResubmission = false; // Tekrar gönderim yasak
        }

        SetModifiedDate();
    }

    /// <summary>
    /// İşlem notları ekle
    /// </summary>
    public void AddProcessingNote(string adminId, string note)
    {
        if (Status != "Processing")
            throw new BusinessRuleException("Not sadece işlemdeki taleplere eklenebilir.");

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
        var formattedNote = $"[{timestamp}] {adminId}: {note}";

        ProcessingNotes = string.IsNullOrWhiteSpace(ProcessingNotes)
            ? formattedNote
            : $"{ProcessingNotes}\n{formattedNote}";

        SetModifiedDate();
    }

    /// <summary>
    /// Acil olarak işaretle
    /// </summary>
    public void MarkAsUrgent(string markedBy, string reason)
    {
        if (IsUrgent)
            return;

        if (Status != VerificationStatuses.Pending && Status != "Processing")
            throw new BusinessRuleException("Sadece bekleyen veya işlemdeki talepler acil işaretlenebilir.");

        IsUrgent = true;
        AddProcessingNote(markedBy, $"ACİL olarak işaretlendi: {reason}");
        SetModifiedDate();

        AddDomainEvent(new VerificationRequestMarkedUrgentEvent(Id, markedBy, reason));
    }

    /// <summary>
    /// Tekrar gönderim iznini güncelle
    /// </summary>
    public void UpdateResubmissionPermission(bool allow, string updatedBy, string reason)
    {
        if (Status != VerificationStatuses.Rejected)
            throw new BusinessRuleException("Sadece reddedilmiş talepler için tekrar gönderim izni güncellenebilir.");

        AllowResubmission = allow;
        AddProcessingNote(updatedBy, $"Tekrar gönderim izni: {(allow ? "VERİLDİ" : "YASAKLANDI")} - {reason}");
        SetModifiedDate();
    }

    /// <summary>
    /// Belge yeniden yüklendi
    /// </summary>
    public void UpdateDocument(string newDocumentUrl, string newDocumentName)
    {
        if (Status != VerificationStatuses.Rejected || !AllowResubmission)
            throw new BusinessRuleException("Belge güncellemesi yapılamaz.");

        ValidateDocumentUrl(newDocumentUrl);
        ValidateDocumentName(newDocumentName);

        DocumentUrl = newDocumentUrl;
        DocumentName = newDocumentName;
        Status = VerificationStatuses.Pending;
        AdminId = null;
        ProcessedAt = null;
        RejectionReason = null;
        ResetApproval(); // ApprovableBaseEntity metodu
        
        SetModifiedDate();

        AddDomainEvent(new VerificationRequestResubmittedEvent(Id, UserId, ReviewId));
    }

    /// <summary>
    /// İşlem süresini tahmin et
    /// </summary>
    public int EstimateProcessingTime()
    {
        return Type switch
        {
            VerificationType.ReviewDocument => IsUrgent ? 2 : 24,      // 2 veya 24 saat
            VerificationType.CompanyDocument => IsUrgent ? 4 : 48,     // 4 veya 48 saat
            VerificationType.UserIdentity => 12,                       // 12 saat
            VerificationType.EmploymentProof => IsUrgent ? 6 : 36,     // 6 veya 36 saat
            _ => 24
        };
    }

    /// <summary>
    /// Talep özetini döndür
    /// </summary>
    public string GetSummary()
    {
        var summary = $"{DocumentType} - {Status}";
        
        if (IsUrgent)
            summary = "🚨 ACİL " + summary;
            
        if (ProcessedAt.HasValue)
        {
            summary += $" (İşlem süresi: {ProcessingTimeHours} saat)";
        }
        else
        {
            var waitingHours = (int)(DateTime.UtcNow - RequestedAt).TotalHours;
            summary += $" (Bekleme: {waitingHours} saat)";
        }

        return summary;
    }

    /// <summary>
    /// SLA (Service Level Agreement) durumu
    /// </summary>
    public bool IsWithinSLA()
    {
        var targetHours = EstimateProcessingTime();
        var actualHours = ProcessedAt.HasValue 
            ? ProcessingTimeHours 
            : (int)(DateTime.UtcNow - RequestedAt).TotalHours;
            
        return actualHours <= targetHours;
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

        // Güvenlik için tehlikeli dosya uzantılarını kontrol et
        var dangerousExtensions = new[] { ".exe", ".bat", ".cmd", ".com", ".scr", ".vbs", ".js" };
        if (dangerousExtensions.Any(ext => name.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            throw new BusinessRuleException("Güvenlik nedeniyle bu dosya türü kabul edilmemektedir.");
    }

    private static void ValidateDocumentType(string type)
    {
        var validTypes = new[]
        {
            DocumentTypes.PaySlip,
            DocumentTypes.EmploymentContract,
            DocumentTypes.EmploymentCertificate,
            DocumentTypes.CompanyIdCard,
            DocumentTypes.SeveranceLetter,
            DocumentTypes.TaxReturn,
            DocumentTypes.TradeRegistry,
            DocumentTypes.Other
        };

        if (!validTypes.Contains(type))
            throw new BusinessRuleException($"Geçersiz belge türü: {type}");
    }
}