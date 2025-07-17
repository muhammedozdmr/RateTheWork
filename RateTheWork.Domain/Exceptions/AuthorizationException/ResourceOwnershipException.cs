namespace RateTheWork.Domain.Exceptions.AuthorizationException;

/// <summary>
/// Kaynak sahipliği exception'ı
/// </summary>
public class ResourceOwnershipException : DomainException
{
    public ResourceOwnershipException
        (string userId, string resourceType, string resourceId, string actualOwnerId, string actionType)
        : base(
            $"User '{userId}' is not the owner of {resourceType} '{resourceId}'. Actual owner is '{actualOwnerId}'. Cannot perform '{actionType}'.")
    {
        UserId = userId;
        ResourceType = resourceType;
        ResourceId = resourceId;
        ActualOwnerId = actualOwnerId;
        ActionType = actionType;
    }

    /// <summary>
    /// Kullanıcı ID'si
    /// </summary>
    public string UserId { get; }

    /// <summary>
    /// Kaynak tipi
    /// </summary>
    public string ResourceType { get; }

    /// <summary>
    /// Kaynak ID'si
    /// </summary>
    public string ResourceId { get; }

    /// <summary>
    /// Gerçek sahip ID'si
    /// </summary>
    public string ActualOwnerId { get; }

    /// <summary>
    /// İşlem tipi
    /// </summary>
    public string ActionType { get; }
}
