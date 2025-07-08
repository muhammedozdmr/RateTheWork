namespace RateTheWork.Domain.Events.Review;

/// <summary>
/// 1. Yorum oluşturuldu event'i
/// </summary>
public record ReviewCreatedEvent(
    string ReviewId,
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
    string ReviewId,
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
    string ReviewId,
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
    string ReviewId,
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
    string ReviewId,
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
    string ReviewId,
    string ActivatedBy,
    DateTime ActivatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}
