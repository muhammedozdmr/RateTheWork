namespace RateTheWork.Domain.Events.Review;

/// <summary>
/// 1. Yorum oluşturuldu event'i
/// </summary>
public class ReviewCreatedEvent : DomainEventBase
{
    public ReviewCreatedEvent
    (
        string? reviewId
        , string userId
        , string companyId
        , string commentType
        , decimal overallRating
        , string? commentText
        , bool hasDocument
        , string? documentUrl
        , Dictionary<string, decimal>? detailedRatings
        , string? sentimentScore
        , decimal? qualityScore
        , string userIp
        , string userAgent
        , DateTime createdAt
    ) : base()
    {
        ReviewId = reviewId;
        UserId = userId;
        CompanyId = companyId;
        CommentType = commentType;
        OverallRating = overallRating;
        CommentText = commentText;
        HasDocument = hasDocument;
        DocumentUrl = documentUrl;
        DetailedRatings = detailedRatings;
        SentimentScore = sentimentScore;
        QualityScore = qualityScore;
        UserIp = userIp;
        UserAgent = userAgent;
        CreatedAt = createdAt;
    }

    public string? ReviewId { get; }
    public string UserId { get; }
    public string CompanyId { get; }
    public string CommentType { get; }
    public decimal OverallRating { get; }
    public string? CommentText { get; }
    public bool HasDocument { get; }
    public string? DocumentUrl { get; }
    public Dictionary<string, decimal>? DetailedRatings { get; }
    public string? SentimentScore { get; }
    public decimal? QualityScore { get; }
    public string UserIp { get; }
    public string UserAgent { get; }
    public DateTime CreatedAt { get; }
}

/// <summary>
/// 2. Yorum güncellendi event'i
/// </summary>
public class ReviewUpdatedEvent : DomainEventBase
{
    public ReviewUpdatedEvent
    (
        string? reviewId
        , string updatedBy
        , string editReason
        , int editCount
        , Dictionary<string, object>? oldValues
        , Dictionary<string, object>? newValues
        , string? moderationNote
        , DateTime updatedAt
    ) : base()
    {
        ReviewId = reviewId;
        UpdatedBy = updatedBy;
        EditReason = editReason;
        EditCount = editCount;
        OldValues = oldValues;
        NewValues = newValues;
        ModerationNote = moderationNote;
        UpdatedAt = updatedAt;
    }

    public string? ReviewId { get; }
    public string UpdatedBy { get; }
    public string EditReason { get; }
    public int EditCount { get; }
    public Dictionary<string, object>? OldValues { get; }
    public Dictionary<string, object>? NewValues { get; }
    public string? ModerationNote { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 3. Yorum belge yüklendi event'i
/// </summary>
public class ReviewDocumentUploadedEvent : DomainEventBase
{
    public ReviewDocumentUploadedEvent
    (
        string? reviewId
        , string documentUrl
        , DateTime uploadedAt
    ) : base()
    {
        ReviewId = reviewId;
        DocumentUrl = documentUrl;
        UploadedAt = uploadedAt;
    }

    public string? ReviewId { get; }
    public string DocumentUrl { get; }
    public DateTime UploadedAt { get; }
}

/// <summary>
/// 4. Yorum doğrulandı event'i
/// </summary>
public class ReviewVerifiedEvent : DomainEventBase
{
    public ReviewVerifiedEvent
    (
        string? reviewId
        , string verifiedBy
        , DateTime verifiedAt
    ) : base()
    {
        ReviewId = reviewId;
        VerifiedBy = verifiedBy;
        VerifiedAt = verifiedAt;
    }

    public string? ReviewId { get; }
    public string VerifiedBy { get; }
    public DateTime VerifiedAt { get; }
}

/// <summary>
/// 5. Yorum gizlendi event'i
/// </summary>
public class ReviewHiddenEvent : DomainEventBase
{
    public ReviewHiddenEvent
    (
        string? reviewId
        , string? hiddenBy
        , string reason
        , bool isAutoHidden
        , string? moderationDetails
        , List<string>? violatedPolicies
        , bool canAppeal
        , DateTime? appealDeadline
        , DateTime hiddenAt
    ) : base()
    {
        ReviewId = reviewId;
        HiddenBy = hiddenBy;
        Reason = reason;
        IsAutoHidden = isAutoHidden;
        ModerationDetails = moderationDetails;
        ViolatedPolicies = violatedPolicies;
        CanAppeal = canAppeal;
        AppealDeadline = appealDeadline;
        HiddenAt = hiddenAt;
    }

    public string? ReviewId { get; }
    public string? HiddenBy { get; }
    public string Reason { get; }
    public bool IsAutoHidden { get; }
    public string? ModerationDetails { get; }
    public List<string>? ViolatedPolicies { get; }
    public bool CanAppeal { get; }
    public DateTime? AppealDeadline { get; }
    public DateTime HiddenAt { get; }
}

/// <summary>
/// 6. Yorum aktifleştirildi event'i
/// </summary>
public class ReviewActivatedEvent : DomainEventBase
{
    public ReviewActivatedEvent
    (
        string? reviewId
        , string activatedBy
        , DateTime activatedAt
    ) : base()
    {
        ReviewId = reviewId;
        ActivatedBy = activatedBy;
        ActivatedAt = activatedAt;
    }

    public string? ReviewId { get; }
    public string ActivatedBy { get; }
    public DateTime ActivatedAt { get; }
}

/// <summary>
/// 7. Mevcut yorumdan yeni yorum oluşturur (farklı bir yorum tipi için)
/// </summary>
public class ReviewCreatedFromTemplateEvent : DomainEventBase
{
    public ReviewCreatedFromTemplateEvent
    (
        string? reviewId
        , string? existingReviewId
        , string commentType
        , DateTime uploadedAt
    ) : base()
    {
        ReviewId = reviewId;
        ExistingReviewId = existingReviewId;
        CommentType = commentType;
        UploadedAt = uploadedAt;
    }

    public string? ReviewId { get; }
    public string? ExistingReviewId { get; }
    public string CommentType { get; }
    public DateTime UploadedAt { get; }
}

/// <summary>
/// 8. Taslak yorum oluşturur (henüz gönderilmemiş)
/// </summary>
public class ReviewDraftCreatedEvent : DomainEventBase
{
    public ReviewDraftCreatedEvent
    (
        string? reviewId
        , string userId
        , string companyId
        , DateTime createdAt
    ) : base()
    {
        ReviewId = reviewId;
        UserId = userId;
        CompanyId = companyId;
        CreatedAt = createdAt;
    }

    public string? ReviewId { get; }
    public string UserId { get; }
    public string CompanyId { get; }
    public DateTime CreatedAt { get; }
}

/// <summary>
/// 9. Yorum faydalılık skoru güncellendi event'i
/// </summary>
public class ReviewHelpfulnessUpdatedEvent : DomainEventBase
{
    public ReviewHelpfulnessUpdatedEvent
    (
        string reviewId
        , decimal newScore
        , decimal oldScore
        , DateTime updatedAt
    ) : base()
    {
        ReviewId = reviewId;
        NewScore = newScore;
        OldScore = oldScore;
        UpdatedAt = updatedAt;
    }

    public string ReviewId { get; }
    public decimal NewScore { get; }
    public decimal OldScore { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 10. Yorum oy sayıları güncellendi event'i
/// </summary>
public class ReviewVoteCountsUpdatedEvent : DomainEventBase
{
    public ReviewVoteCountsUpdatedEvent
    (
        string reviewId
        , int upvotes
        , int downvotes
        , DateTime updatedAt
    ) : base()
    {
        ReviewId = reviewId;
        Upvotes = upvotes;
        Downvotes = downvotes;
        UpdatedAt = updatedAt;
    }

    public string ReviewId { get; }
    public int Upvotes { get; }
    public int Downvotes { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 11. Yorum şirket ID'si güncellendi event'i (şirket birleşmeleri için)
/// </summary>
public class ReviewCompanyUpdatedEvent : DomainEventBase
{
    public ReviewCompanyUpdatedEvent
    (
        string reviewId
        , string oldCompanyId
        , string newCompanyId
        , string updatedBy
        , DateTime updatedAt
    ) : base()
    {
        ReviewId = reviewId;
        OldCompanyId = oldCompanyId;
        NewCompanyId = newCompanyId;
        UpdatedBy = updatedBy;
        UpdatedAt = updatedAt;
    }

    public string ReviewId { get; }
    public string OldCompanyId { get; }
    public string NewCompanyId { get; }
    public string UpdatedBy { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 12. Yorum rapor edildi event'i
/// </summary>
public class ReviewReportedEvent : DomainEventBase
{
    public ReviewReportedEvent
    (
        string? reviewId
        , string reportedBy
        , string reportReason
        , string? additionalDetails
        , bool isAutoDetected
        , decimal? confidenceScore
        , DateTime reportedAt
    ) : base()
    {
        ReviewId = reviewId;
        ReportedBy = reportedBy;
        ReportReason = reportReason;
        AdditionalDetails = additionalDetails;
        IsAutoDetected = isAutoDetected;
        ConfidenceScore = confidenceScore;
        ReportedAt = reportedAt;
    }

    public string? ReviewId { get; }
    public string ReportedBy { get; }
    public string ReportReason { get; }
    public string? AdditionalDetails { get; }
    public bool IsAutoDetected { get; }
    public decimal? ConfidenceScore { get; }
    public DateTime ReportedAt { get; }
}

/// <summary>
/// 13. Yorum öne çıkarıldı event'i
/// </summary>
public class ReviewFeaturedEvent : DomainEventBase
{
    public ReviewFeaturedEvent
    (
        string? reviewId
        , string featuredBy
        , string? featuredReason
        , DateTime? featuredUntil
        , int displayOrder
        , DateTime featuredAt
    ) : base()
    {
        ReviewId = reviewId;
        FeaturedBy = featuredBy;
        FeaturedReason = featuredReason;
        FeaturedUntil = featuredUntil;
        DisplayOrder = displayOrder;
        FeaturedAt = featuredAt;
    }

    public string? ReviewId { get; }
    public string FeaturedBy { get; }
    public string? FeaturedReason { get; }
    public DateTime? FeaturedUntil { get; }
    public int DisplayOrder { get; }
    public DateTime FeaturedAt { get; }
}

/// <summary>
/// 14. Yorum çevrildi event'i
/// </summary>
public class ReviewTranslatedEvent : DomainEventBase
{
    public ReviewTranslatedEvent
    (
        string? reviewId
        , string sourceLanguage
        , string targetLanguage
        , string translationProvider
        , bool isAutoTranslated
        , DateTime translatedAt
    ) : base()
    {
        ReviewId = reviewId;
        SourceLanguage = sourceLanguage;
        TargetLanguage = targetLanguage;
        TranslationProvider = translationProvider;
        IsAutoTranslated = isAutoTranslated;
        TranslatedAt = translatedAt;
    }

    public string? ReviewId { get; }
    public string SourceLanguage { get; }
    public string TargetLanguage { get; }
    public string TranslationProvider { get; }
    public bool IsAutoTranslated { get; }
    public DateTime TranslatedAt { get; }
}

/// <summary>
/// 15. Yoruma yanıt verildi event'i
/// </summary>
public class ReviewCommentedEvent : DomainEventBase
{
    public ReviewCommentedEvent
    (
        string? reviewId
        , string commentId
        , string commentedBy
        , string commentText
        , bool isCompanyResponse
        , DateTime commentedAt
    ) : base()
    {
        ReviewId = reviewId;
        CommentId = commentId;
        CommentedBy = commentedBy;
        CommentText = commentText;
        IsCompanyResponse = isCompanyResponse;
        CommentedAt = commentedAt;
    }

    public string? ReviewId { get; }
    public string CommentId { get; }
    public string CommentedBy { get; }
    public string CommentText { get; }
    public bool IsCompanyResponse { get; }
    public DateTime CommentedAt { get; }
}
