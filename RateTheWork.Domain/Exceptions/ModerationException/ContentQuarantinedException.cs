namespace RateTheWork.Domain.Exceptions.ModerationException;

/// <summary>
/// İçerik karantinada exception'ı
/// </summary>
public class ContentQuarantinedException : DomainException
{
    public Guid ContentId { get; }
    public string ContentType { get; }
    public string QuarantineReason { get; }
    public DateTime ReviewDeadline { get; }

    public ContentQuarantinedException(Guid contentId, string contentType, string quarantineReason, DateTime reviewDeadline)
        : base($"{contentType} is in quarantine for review. Reason: {quarantineReason}. Review deadline: {reviewDeadline:yyyy-MM-dd HH:mm}")
    {
        ContentId = contentId;
        ContentType = contentType;
        QuarantineReason = quarantineReason;
        ReviewDeadline = reviewDeadline;
    }
}
