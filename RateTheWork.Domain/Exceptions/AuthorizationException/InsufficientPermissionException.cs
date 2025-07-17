namespace RateTheWork.Domain.Exceptions.AuthorizationException;

/// <summary>
/// Yetersiz yetki exception'ı
/// </summary>
public class InsufficientPermissionException : DomainException
{
    public InsufficientPermissionException
        (string userId, string requiredPermission, string actionType, string resourceType, string? resourceId = null)
        : base(
            $"User '{userId}' does not have permission '{requiredPermission}' to perform '{actionType}' on {resourceType}")
    {
        UserId = userId;
        RequiredPermission = requiredPermission;
        ActionType = actionType;
        ResourceType = resourceType;
        ResourceId = resourceId;
    }

    /// <summary>
    /// Kullanıcı ID'si
    /// </summary>
    public string UserId { get; }

    /// <summary>
    /// Gereken yetki
    /// </summary>
    public string RequiredPermission { get; }

    /// <summary>
    /// İşlem tipi
    /// </summary>
    public string ActionType { get; }

    /// <summary>
    /// Kaynak tipi
    /// </summary>
    public string ResourceType { get; }

    /// <summary>
    /// Kaynak ID'si
    /// </summary>
    public string? ResourceId { get; }
}
