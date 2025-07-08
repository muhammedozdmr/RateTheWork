namespace RateTheWork.Domain.Exceptions.ModerationException;

/// <summary>
/// Moderasyon reddi exception'ı
/// </summary>
public class ModerationRejectedException : DomainException
{
    public Guid ContentId { get; }
    public string ContentType { get; }
    public string[] RejectionReasons { get; }
    public string ModeratorNote { get; }

    public ModerationRejectedException(Guid contentId, string contentType, string[] rejectionReasons, string moderatorNote = null)
        : base($"{contentType} rejected by moderation. Reasons: {string.Join(", ", rejectionReasons)}")
    {
        ContentId = contentId;
        ContentType = contentType;
        RejectionReasons = rejectionReasons;
        ModeratorNote = moderatorNote;
    }
}
