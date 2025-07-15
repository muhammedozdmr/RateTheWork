using RateTheWork.Domain.Common;
using RateTheWork.Domain.Constants;
using RateTheWork.Domain.Events.Review;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.Common;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Yorum entity'si - Bir şirkete yapılan yorumu ve puanlamayı temsil eder.
/// Rich domain model ile iş kurallarını içerir.
/// </summary>
public class Review : AuditableBaseEntity, IAggregateRoot
{
    // Properties
    public string CompanyId { get; private set; } = string.Empty;
    public string UserId { get; private set; } = string.Empty;
    public string CommentType { get; private set; } = string.Empty;
    public decimal OverallRating { get; private set; }
    public string CommentText { get; private set; } = string.Empty;
    public string? DocumentUrl { get; private set; }
    public bool IsDocumentVerified { get; private set; } = false;
    public int Upvotes { get; private set; } = 0;
    public int Downvotes { get; private set; } = 0;
    public int ReportCount { get; private set; } = 0;
    public bool IsActive { get; private set; } = true;
    public DateTime? LastEditedAt { get; private set; }
    public string? EditReason { get; private set; }
    public int EditCount { get; private set; } = 0;
    public decimal HelpfulnessScore { get; private set; } = 0; // Faydalılık skoru
    public DateTime? UpdatedAt { get; private set; } // Güncelleme zamanı
    public string? TargetType { get; private set; } // Hedef tipi (örn: "Company", "Branch")
    public string? TargetId { get; private set; } // Hedef ID'si

    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private Review() : base()
    {
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
        , string commentText
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
            , IsDocumentVerified = false, IsActive = true, Upvotes = 0, Downvotes = 0, ReportCount = 0, EditCount = 0
        };

        // Domain Event
        review.AddDomainEvent(new ReviewCreatedEvent(
            review.Id,
            userId,
            companyId,
            commentType,
            overallRating,
            !string.IsNullOrEmpty(documentUrl),
            DateTime.UtcNow
        ));

        return review;
    }

    /// <summary>
    /// Taslak yorum oluşturur (henüz gönderilmemiş)
    /// </summary>
    public static Review CreateDraft(string companyId, string userId, string commentType)
    {
        var review = new Review
        {
            CompanyId = companyId, UserId = userId, CommentType = commentType, CommentText = string.Empty
            , OverallRating = 0, // Henüz puanlanmamış
            IsActive = false
            , // Draft olduğu için inactive
            // IsDraft = true // Eğer böyle bir property varsa
        };

        review.AddDomainEvent(new ReviewDraftCreatedEvent(review.Id, userId, companyId, DateTime.UtcNow));

        return review;
    }

    /// <summary>
    /// Mevcut yorumdan yeni yorum oluşturur (farklı bir yorum tipi için)
    /// </summary>
    public static Review CreateFromExisting(Review existingReview, string newCommentType)
    {
        if (existingReview.CommentType == newCommentType)
            throw new BusinessRuleException("Aynı tip için kopyalama yapılamaz.");

        var review = new Review
        {
            CompanyId = existingReview.CompanyId, UserId = existingReview.UserId, CommentType = newCommentType
            , CommentText = string.Empty, // Yeni yorum metni girilmeli
            OverallRating = existingReview.OverallRating
            , // Aynı puan ile başla
            IsActive = true
        };

        review.AddDomainEvent(new ReviewCreatedFromTemplateEvent(
            review.Id,
            existingReview.Id,
            newCommentType,
            DateTime.UtcNow
        ));

        return review;
    }

    /// <summary>
    /// Minimum bilgi ile hızlı yorum oluşturur
    /// </summary>
    public static Review CreateQuick(string companyId, string userId, decimal rating)
    {
        var review = Create(
            companyId,
            userId,
            "WorkEnvironment", // Default yorum tipi
            rating,
            GenerateMinimumComment() // Minimum karakter sayısını karşılayan otomatik metin
        );

        return review;
    }

    private static string GenerateMinimumComment()
    {
        return "Bu şirkette çalışma deneyimim genel olarak olumlu/olumsuz geçti. " +
               "Çalışma ortamı ve iş arkadaşları açısından değerlendirdiğimde, " +
               "şirketin güçlü ve geliştirilmesi gereken yönleri bulunmaktadır."; // 50+ karakter
    }

    /// <summary>
    /// Yorumu günceller
    /// </summary>
    public void Update(string newCommentText, string editReason)
    {
        // Düzenleme süresi kontrolü
        var hoursSinceCreation = (DateTime.UtcNow - CreatedAt).TotalHours;
        if (hoursSinceCreation > DomainConstants.Review.MaxEditHours)
            throw new BusinessRuleException(
                $"Yorum oluşturulduktan {DomainConstants.Review.MaxEditHours} saat sonra düzenlenemez.");

        if (EditCount >= DomainConstants.Review.MaxEditCount)
            throw new BusinessRuleException(
                $"Bir yorum en fazla {DomainConstants.Review.MaxEditCount} kez düzenlenebilir.");

        ValidateCommentText(newCommentText);
        ValidateEditReason(editReason);

        CommentText = newCommentText;
        EditReason = editReason;
        LastEditedAt = DateTime.UtcNow;
        EditCount++;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new ReviewUpdatedEvent(
            Id,
            UserId,
            editReason,
            EditCount,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Belge yükler
    /// </summary>
    public void UploadDocument(string documentUrl)
    {
        if (string.IsNullOrWhiteSpace(documentUrl))
            throw new ArgumentNullException(nameof(documentUrl));

        if (!string.IsNullOrEmpty(DocumentUrl))
            throw new BusinessRuleException("Bu yorum için zaten bir belge yüklenmiş.");

        DocumentUrl = documentUrl;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new ReviewDocumentUploadedEvent(
            Id,
            documentUrl,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Belgeyi doğrula
    /// </summary>
    public void VerifyDocument(string verifiedBy)
    {
        if (string.IsNullOrEmpty(DocumentUrl))
            throw new BusinessRuleException("Doğrulanacak belge bulunamadı.");

        if (IsDocumentVerified)
            throw new BusinessRuleException("Belge zaten doğrulanmış.");

        IsDocumentVerified = true;
        SetModifiedAudit(verifiedBy);

        // Domain Event
        AddDomainEvent(new ReviewVerifiedEvent(
            Id,
            verifiedBy,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Upvote sayısını artırır
    /// </summary>
    public void IncrementUpvotes()
    {
        Upvotes++;
        SetModifiedDate();
    }

    /// <summary>
    /// Downvote sayısını artırır
    /// </summary>
    public void IncrementDownvotes()
    {
        Downvotes++;
        SetModifiedDate();
    }

    /// <summary>
    /// Upvote sayısını azaltır
    /// </summary>
    public void DecrementUpvotes()
    {
        if (Upvotes > 0)
        {
            Upvotes--;
            SetModifiedDate();
        }
    }

    /// <summary>
    /// Downvote sayısını azaltır
    /// </summary>
    public void DecrementDownvotes()
    {
        if (Downvotes > 0)
        {
            Downvotes--;
            SetModifiedDate();
        }
    }

    /// <summary>
    /// Şikayet sayısını artırır
    /// </summary>
    public void IncrementReportCount()
    {
        ReportCount++;
        SetModifiedDate();

        // Otomatik gizleme kontrolü
        if (ReportCount >= DomainConstants.Review.MaxReportCountBeforeAutoHide && IsActive)
        {
            Hide(null, "Çok fazla şikayet nedeniyle otomatik gizlendi", true);
        }
    }

    /// <summary>
    /// Yorumu gizle
    /// </summary>
    public void Hide(string? hiddenBy, string reason, bool isAutoHidden = false)
    {
        if (!IsActive)
            throw new BusinessRuleException("Yorum zaten gizli.");

        IsActive = false;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new ReviewHiddenEvent(
            Id,
            hiddenBy,
            reason,
            isAutoHidden,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Yorumu aktifleştir
    /// </summary>
    public void Activate(string activatedBy)
    {
        if (IsActive)
            throw new BusinessRuleException("Yorum zaten aktif.");

        IsActive = true;
        ReportCount = 0; // Aktivasyon sonrası şikayet sayısı sıfırlanır
        SetModifiedAudit(activatedBy);

        // Domain Event
        AddDomainEvent(new ReviewActivatedEvent(
            Id,
            activatedBy,
            DateTime.UtcNow
        ));
    }

    // HelpfulnessScore güncelleme metodu
    public void UpdateHelpfulnessScore(decimal score)
    {
        var oldScore = HelpfulnessScore; // Eski skoru sakla
        HelpfulnessScore = score;
        SetModifiedDate();

        if (score < 0 || score > 100)
            throw new ArgumentException("Helpfulness score must be between 0 and 100");

        HelpfulnessScore = score;
        UpdatedAt = DateTime.UtcNow;

        if (Id != null)
            AddDomainEvent(new ReviewHelpfulnessUpdatedEvent(
                Id,
                oldScore,
                score,
                DateTime.UtcNow,
                DateTime.UtcNow));
    }

    /// <summary>
    /// Faydalılık skorunu hesaplar
    /// </summary>
    public decimal CalculateHelpfulnessScore()
    {
        var totalVotes = Upvotes + Downvotes;
        if (totalVotes == 0)
            return 0;

        var baseScore = (decimal)Upvotes / totalVotes * 100;

        // Doğrulanmış yorumlar için bonus
        if (IsDocumentVerified)
            baseScore *= 1.2m;

        return Math.Round(baseScore, 2);
    }

    // Private validation methods
    private static void ValidateRating(decimal rating)
    {
        if (rating < DomainConstants.Review.MinRating || rating > DomainConstants.Review.MaxRating)
            throw new BusinessRuleException(
                $"Puan {DomainConstants.Review.MinRating} ile {DomainConstants.Review.MaxRating} arasında olmalıdır.");

        // 0.5'lik artışlarla sınırla
        if (rating % 0.5m != 0)
            throw new BusinessRuleException("Puan 0.5'lik artışlarla verilebilir.");
    }

    private static void ValidateCommentText(string commentText)
    {
        if (string.IsNullOrWhiteSpace(commentText))
            throw new ArgumentNullException(nameof(commentText));

        if (commentText.Length < DomainConstants.Review.MinCommentLength)
            throw new BusinessRuleException(
                $"Yorum en az {DomainConstants.Review.MinCommentLength} karakter olmalıdır.");

        if (commentText.Length > DomainConstants.Review.MaxCommentLength)
            throw new BusinessRuleException(
                $"Yorum en fazla {DomainConstants.Review.MaxCommentLength} karakter olabilir.");
    }

    private static void ValidateCommentType(string commentType)
    {
        if (string.IsNullOrWhiteSpace(commentType))
            throw new ArgumentNullException(nameof(commentType));

        var validTypes = new[]
            { "Salary", "WorkEnvironment", "CareerGrowth", "Benefits", "Management", "WorkLifeBalance" };
        if (!validTypes.Contains(commentType))
            throw new BusinessRuleException("Geçersiz yorum türü.");
    }

    private static void ValidateEditReason(string editReason)
    {
        if (string.IsNullOrWhiteSpace(editReason))
            throw new ArgumentNullException(nameof(editReason));

        if (editReason.Length < 10)
            throw new BusinessRuleException("Düzenleme nedeni en az 10 karakter olmalıdır.");
    }
}
