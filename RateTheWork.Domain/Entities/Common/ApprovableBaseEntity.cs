namespace RateTheWork.Domain.Entities.Common;

/// <summary>
/// Onay gerektiren entity'ler için (Company, Review vs.)
/// </summary>
public abstract class ApprovableBaseEntity : AuditableBaseEntity
{
    /// <summary>
    /// Entity onaylandı mı?
    /// </summary>
    public bool IsApproved { get; set; } = false;
    
    /// <summary>
    /// Onaylayan kullanıcının ID'si
    /// </summary>
    public string? ApprovedBy { get; set; }
    
    /// <summary>
    /// Onaylanma tarihi
    /// </summary>
    public DateTime? ApprovedAt { get; set; }
    
    /// <summary>
    /// Onay notları (red nedeni vs.)
    /// </summary>
    public string? ApprovalNotes { get; set; }
}
