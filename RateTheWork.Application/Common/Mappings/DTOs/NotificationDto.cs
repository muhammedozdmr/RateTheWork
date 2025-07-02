namespace RateTheWork.Application.Common.Mappings.DTOs;

/// <summary>
/// Bildirim DTO'su
/// </summary>
public record NotificationDto
{
    /// <summary>
    /// Bildirim ID'si
    /// </summary>
    public string NotificationId { get; init; } = string.Empty;
    
    /// <summary>
    /// Bildirim türü
    /// </summary>
    public string Type { get; init; } = string.Empty;
    
    /// <summary>
    /// Bildirim mesajı
    /// </summary>
    public string Message { get; init; } = string.Empty;
    
    /// <summary>
    /// Okundu mu?
    /// </summary>
    public bool IsRead { get; init; }
    
    /// <summary>
    /// Oluşturulma tarihi
    /// </summary>
    public DateTime CreatedDate { get; init; }
    
    /// <summary>
    /// İlgili entity ID'si (opsiyonel)
    /// </summary>
    public string? RelatedEntityId { get; init; }
    
    /// <summary>
    /// İlgili entity türü (opsiyonel)
    /// </summary>
    public string? RelatedEntityType { get; init; }
}