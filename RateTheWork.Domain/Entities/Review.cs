using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums;
using RateTheWork.Domain.Events;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Yorum entity'si - Bir şirkete yapılan yorumu ve puanlamayı temsil eder.
/// Rich domain model ile iş kurallarını içerir.
/// </summary>
public class Review : AuditableBaseEntity
{
    // Constants - İş kuralları
    private const int MinCommentLength = 50;
    private const int MaxCommentLength = 2000;
    private const decimal MinRating = 1.0m;
    private const decimal MaxRating = 5.0m;
    private const int MaxReportCountBeforeAutoHide = 5;
    private const int MaxEditHours = 24; // Yorum düzenleme süresi

    // Properties
    public string? CompanyId { get; private set; } = string.Empty;
    public string? UserId { get; private set; } = string.Empty;
    public string? CommentType { get; private set; } = string.Empty;
    public decimal OverallRating { get; private set; }
    public string? CommentText { get; private set; } = string.Empty;
    public string? DocumentUrl { get; private set; } = string.Empty;
    public bool IsDocumentVerified { get; private set; }
    public int Upvotes { get; private set; }
    public int Downvotes { get; private set; }
    public int ReportCount { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastEditedAt { get; private set; }
    public string? EditReason { get; private set; } = string.Empty;
    public int EditCount { get; private set; }

    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private Review() : base()
    {
    }

    /// <summary>
    /// EF Core için private constructor
    /// </summary>
    private Review(string? companyId, string? userId, string? commentType, string? commentText) : base()
    {
        CompanyId = companyId;
        UserId = userId;
        CommentType = commentType;
        CommentText = commentText;
    }

    /// <summary>
    /// Yeni yorum oluşturur (Factory method)
    /// </summary>
    public static Review Create
    (
        string companyId
        , string userId
        , string commentType
        , decimal overallRating
        , string? commentText
        , string? documentUrl = null
    )
    {
        // Validasyonlar
        ValidateRating(overallRating);
        ValidateCommentText(commentText);
        ValidateCommentType(commentType);

        var review = new Review
        {
            CompanyId = companyId ?? throw new ArgumentNullException(nameof(companyId))
            , UserId = userId ?? throw new ArgumentNullException(nameof(userId)), CommentType = commentType
            , OverallRating = overallRating, CommentText = commentText, DocumentUrl = documentUrl
            , IsDocumentVerified = false, Upvotes = 0, Downvotes = 0, ReportCount = 0, IsActive = true, EditCount = 0
        };

        // Domain Event
        review.AddDomainEvent(new ReviewCreatedEvent(
            review.Id,
            userId,
            companyId,
            overallRating,
            DateTime.UtcNow
        ));

        return review;
    }

    /// <summary>
    /// Yorumu düzenler
    /// </summary>
    public void Edit(string? newCommentText, string editReason, string editorUserId)
    {
        // Yetki kontrolü
        if (UserId != editorUserId)
            throw new BusinessRuleException("Sadece yorum sahibi düzenleme yapabilir.");

        // Süre kontrolü
        var hoursSinceCreation = (DateTime.UtcNow - CreatedAt).TotalHours;
        if (hoursSinceCreation > MaxEditHours)
            throw new BusinessRuleException($"Yorumlar yalnızca ilk {MaxEditHours} saat içinde düzenlenebilir.");

        // Validasyon
        ValidateCommentText(newCommentText);

        if (string.IsNullOrWhiteSpace(editReason))
            throw new BusinessRuleException("Düzenleme nedeni belirtilmelidir.");

        CommentText = newCommentText;
        EditReason = editReason;
        LastEditedAt = DateTime.UtcNow;
        EditCount++;
        SetModifiedDate();

        AddDomainEvent(new ReviewEditedEvent(Id, editorUserId, editReason));
    }

    /// <summary>
    /// Yoruma oy ekler
    /// </summary>
    public void AddVote(bool isUpvote)
    {
        if (!IsActive)
            throw new BusinessRuleException("Aktif olmayan yoruma oy verilemez.");

        if (isUpvote)
            Upvotes++;
        else
            Downvotes++;

        SetModifiedDate();
    }

    /// <summary>
    /// Yorumdan oy kaldırır
    /// </summary>
    public void RemoveVote(bool wasUpvote)
    {
        if (wasUpvote && Upvotes > 0)
            Upvotes--;
        else if (!wasUpvote && Downvotes > 0)
            Downvotes--;

        SetModifiedDate();
    }

    /// <summary>
    /// Oy değiştirir (upvote->downvote veya tersi)
    /// </summary>
    public void ChangeVote(bool fromUpvote)
    {
        if (fromUpvote)
        {
            if (Upvotes > 0) Upvotes--;
            Downvotes++;
        }
        else
        {
            if (Downvotes > 0) Downvotes--;
            Upvotes++;
        }

        SetModifiedDate();
    }

    /// <summary>
    /// Yoruma şikayet ekler
    /// </summary>
    public void AddReport(string reporterId, string reason)
    {
        if (!IsActive)
            throw new BusinessRuleException("Aktif olmayan yorum şikayet edilemez.");

        if (UserId == reporterId)
            throw new BusinessRuleException("Kendi yorumunuzu şikayet edemezsiniz.");

        ReportCount++;

        // Otomatik gizleme kontrolü
        if (ReportCount >= MaxReportCountBeforeAutoHide)
        {
            Hide($"Otomatik gizlendi: {MaxReportCountBeforeAutoHide} şikayet sınırı aşıldı");
        }

        AddDomainEvent(new ReviewReportedEvent(Id, reporterId, reason, ReportCount));
    }

    /// <summary>
    /// Belge ekler
    /// </summary>
    public void AttachDocument(string documentUrl)
    {
        if (!string.IsNullOrWhiteSpace(DocumentUrl))
            throw new BusinessRuleException("Bu yorumda zaten bir belge mevcut.");

        DocumentUrl = documentUrl ?? throw new ArgumentNullException(nameof(documentUrl));
        IsDocumentVerified = false;
        SetModifiedDate();

        AddDomainEvent(new ReviewDocumentAttachedEvent(Id, documentUrl));
    }

    /// <summary>
    /// Belgeyi doğrular
    /// </summary>
    public void VerifyDocument(string adminId)
    {
        if (string.IsNullOrWhiteSpace(DocumentUrl))
            throw new BusinessRuleException("Doğrulanacak belge bulunamadı.");

        if (IsDocumentVerified)
            throw new BusinessRuleException("Belge zaten doğrulanmış.");

        IsDocumentVerified = true;
        SetModifiedDate();

        AddDomainEvent(new ReviewDocumentVerifiedEvent(Id, adminId, DocumentUrl!));
    }

    /// <summary>
    /// Yorumu gizler
    /// </summary>
    public void Hide(string reason)
    {
        if (!IsActive)
            return; // Zaten gizli

        IsActive = false;
        SetModifiedDate();

        AddDomainEvent(new ReviewHiddenEvent(Id, reason));
    }

    /// <summary>
    /// Yorumu tekrar aktif eder
    /// </summary>
    public void Activate(string adminId, string reason)
    {
        if (IsActive)
            return; // Zaten aktif

        IsActive = true;
        ReportCount = 0; // Şikayetleri sıfırla
        SetModifiedDate();

        AddDomainEvent(new ReviewActivatedEvent(Id, adminId, reason));
    }

    /// <summary>
    /// Net oy sayısını hesaplar
    /// </summary>
    public int GetNetVotes() => Upvotes - Downvotes;

    /// <summary>
    /// Yararlılık skorunu hesaplar (0-100 arası)
    /// </summary>
    public int CalculateHelpfulnessScore()
    {
        var totalVotes = Upvotes + Downvotes;
        if (totalVotes == 0) return 50; // Varsayılan skor

        var score = (Upvotes * 100) / totalVotes;
        return Math.Max(0, Math.Min(100, score));
    }

    // Private validation methods
    private static void ValidateRating(decimal rating)
    {
        if (rating < MinRating || rating > MaxRating)
            throw new BusinessRuleException($"Puan {MinRating} ile {MaxRating} arasında olmalıdır.");
    }

    private static void ValidateCommentText(string? commentText)
    {
        if (string.IsNullOrWhiteSpace(commentText))
            throw new BusinessRuleException("Yorum metni boş olamaz.");

        if (commentText.Length < MinCommentLength)
            throw new BusinessRuleException($"Yorum en az {MinCommentLength} karakter olmalıdır.");

        if (commentText.Length > MaxCommentLength)
            throw new BusinessRuleException($"Yorum en fazla {MaxCommentLength} karakter olabilir.");
    }

    private static void ValidateCommentType(string commentType)
    {
        if (!CommentTypes.IsValid(commentType))
            throw new BusinessRuleException($"Geçersiz yorum türü: {commentType}");
    }
}
