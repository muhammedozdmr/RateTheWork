namespace RateTheWork.Application.Common.Mappings.DTOs;

/// <summary>
/// Audit log DTO'su (Admin panel için)
/// </summary>
public record AuditLogDto
{
    /// <summary>
    /// Log ID'si
    /// </summary>
    public string LogId { get; init; } = string.Empty;
    
    /// <summary>
    /// İşlemi yapan admin kullanıcı adı
    /// </summary>
    public string AdminUsername { get; init; } = string.Empty;
    
    /// <summary>
    /// İşlem türü
    /// </summary>
    public string ActionType { get; init; } = string.Empty;
    
    /// <summary>
    /// Etkilenen entity türü
    /// </summary>
    public string EntityType { get; init; } = string.Empty;
    
    /// <summary>
    /// Etkilenen entity ID'si
    /// </summary>
    public string EntityId { get; init; } = string.Empty;
    
    /// <summary>
    /// İşlem detayları
    /// </summary>
    public string? Details { get; init; }
    
    /// <summary>
    /// İşlem tarihi
    /// </summary>
    public DateTime ActionDate { get; init; }
}
