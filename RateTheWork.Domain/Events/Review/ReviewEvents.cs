namespace RateTheWork.Domain.Events.Review;

/// <summary>
/// 1. Yorum oluşturuldu event'i
/// </summary>
public record ReviewCreatedEvent(
    string? ReviewId
    , string UserId
    , string CompanyId
    , string CommentType
    , decimal OverallRating
    , string? CommentText
    , bool HasDocument
    , string? DocumentUrl
    , Dictionary<string, decimal>? DetailedRatings
    , string? SentimentScore
    , decimal? QualityScore
    , string UserIp
    , string UserAgent
    , DateTime CreatedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 2. Yorum güncellendi event'i
/// </summary>
public record ReviewUpdatedEvent(
    string? ReviewId
    , string UpdatedBy
    , string EditReason
    , int EditCount
    , Dictionary<string, object>? OldValues
    , Dictionary<string, object>? NewValues
    , string? ModerationNote
    , DateTime UpdatedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 3. Yorum belge yüklendi event'i
/// </summary>
public record ReviewDocumentUploadedEvent(
    string? ReviewId
    , string DocumentUrl
    , DateTime UploadedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 4. Yorum doğrulandı event'i
/// </summary>
public record ReviewVerifiedEvent(
    string? ReviewId
    , string VerifiedBy
    , DateTime VerifiedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 5. Yorum gizlendi event'i
/// </summary>
public record ReviewHiddenEvent(
    string? ReviewId
    , string? HiddenBy
    , string Reason
    , bool IsAutoHidden
    , string? ModerationDetails
    , List<string>? ViolatedPolicies
    , bool CanAppeal
    , DateTime? AppealDeadline
    , DateTime HiddenAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 6. Yorum aktifleştirildi event'i
/// </summary>
public record ReviewActivatedEvent(
    string? ReviewId
    , string ActivatedBy
    , DateTime ActivatedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 7. Mevcut yorumdan yeni yorum oluşturur (farklı bir yorum tipi için)
/// </summary>
public record ReviewCreatedFromTemplateEvent(
    string? ReviewId
    , string? ExistingReviewId
    , string CommentType
    , DateTime UploadedAt
    , DateTime OccurredOn = default) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 8. Taslak yorum oluşturur (henüz gönderilmemiş)
/// </summary>
public record ReviewDraftCreatedEvent(
    string? ReviewId
    , string UserId
    , string CompanyId
    , DateTime CreatedAt
    , DateTime OccurredOn = default) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 9. Yorum faydalılık skoru güncellendi event'i
/// </summary>
public record ReviewHelpfulnessUpdatedEvent(
    string ReviewId
    , decimal NewScore
    , decimal OldScore
    , DateTime UpdatedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 10. Yorum oy sayıları güncellendi event'i
/// </summary>
public record ReviewVoteCountsUpdatedEvent(
    string ReviewId
    , int Upvotes
    , int Downvotes
    , DateTime UpdatedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 11. Yorum şirket ID'si güncellendi event'i (şirket birleşmeleri için)
/// </summary>
public record ReviewCompanyUpdatedEvent(
    string ReviewId
    , string OldCompanyId
    , string NewCompanyId
    , string UpdatedBy
    , DateTime UpdatedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 12. Yorum rapor edildi event'i
/// </summary>
public record ReviewReportedEvent(
    string? ReviewId
    , string ReportedBy
    , string ReportReason
    , string? AdditionalDetails
    , bool IsAutoDetected
    , decimal? ConfidenceScore
    , DateTime ReportedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 13. Yorum öne çıkarıldı event'i
/// </summary>
public record ReviewFeaturedEvent(
    string? ReviewId
    , string FeaturedBy
    , string? FeaturedReason
    , DateTime? FeaturedUntil
    , int DisplayOrder
    , DateTime FeaturedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 14. Yorum çevrildi event'i
/// </summary>
public record ReviewTranslatedEvent(
    string? ReviewId
    , string SourceLanguage
    , string TargetLanguage
    , string TranslationProvider
    , bool IsAutoTranslated
    , DateTime TranslatedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 15. Yoruma yanıt verildi event'i
/// </summary>
public record ReviewCommentedEvent(
    string? ReviewId
    , string CommentId
    , string CommentedBy
    , string CommentText
    , bool IsCompanyResponse
    , DateTime CommentedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}
