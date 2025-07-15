namespace RateTheWork.Domain.Events.Review;

/// <summary>
/// 1. Yorum oluşturuldu event'i
/// </summary>
public record ReviewCreatedEvent(
    string? ReviewId,
    string UserId,
    string CompanyId,
    string CommentType,
    decimal OverallRating,
    bool HasDocument,
    DateTime CreatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 2. Yorum güncellendi event'i
/// </summary>
public record ReviewUpdatedEvent(
    string? ReviewId,
    string UpdatedBy,
    string EditReason,
    int EditCount,
    DateTime UpdatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 3. Yorum belge yüklendi event'i
/// </summary>
public record ReviewDocumentUploadedEvent(
    string? ReviewId,
    string DocumentUrl,
    DateTime UploadedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 4. Yorum doğrulandı event'i
/// </summary>
public record ReviewVerifiedEvent(
    string? ReviewId,
    string VerifiedBy,
    DateTime VerifiedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 5. Yorum gizlendi event'i
/// </summary>
public record ReviewHiddenEvent(
    string? ReviewId,
    string? HiddenBy,
    string Reason,
    bool IsAutoHidden,
    DateTime HiddenAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 6. Yorum aktifleştirildi event'i
/// </summary>
public record ReviewActivatedEvent(
    string? ReviewId,
    string ActivatedBy,
    DateTime ActivatedAt,
    DateTime OccurredOn = default
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
    ,  DateTime CreatedAt
    , DateTime OccurredOn = default) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 9. Yorum faydalılık skoru güncellendi event'i
/// </summary>
public record ReviewHelpfulnessUpdatedEvent(
    string ReviewId,
    decimal NewScore,
    decimal OldScore,
    DateTime UpdatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 10. Yorum oy sayıları güncellendi event'i
/// </summary>
public record ReviewVoteCountsUpdatedEvent(
    string ReviewId,
    int Upvotes,
    int Downvotes,
    DateTime UpdatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 11. Yorum şirket ID'si güncellendi event'i (şirket birleşmeleri için)
/// </summary>
public record ReviewCompanyUpdatedEvent(
    string ReviewId,
    string OldCompanyId,
    string NewCompanyId,
    string UpdatedBy,
    DateTime UpdatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}
