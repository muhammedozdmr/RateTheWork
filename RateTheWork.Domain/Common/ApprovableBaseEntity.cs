namespace RateTheWork.Domain.Common;

/// <summary>
/// Onay gerektiren entity'ler için base class.
/// Company, Review gibi admin onayı gerektiren entity'ler bu sınıftan türer.
/// </summary>
public abstract class ApprovableBaseEntity : AuditableBaseEntity
{
    /// <summary>
    /// Entity onaylandı mı?
    /// </summary>
    public bool IsApproved { get; protected set; }
    
    /// <summary>
    /// Onaylayan kullanıcının ID'si
    /// </summary>
    public string? ApprovedBy { get; protected set; }
    
    /// <summary>
    /// Onaylanma tarihi
    /// </summary>
    public DateTime? ApprovedAt { get; protected set; }
    
    /// <summary>
    /// Onay notları (red nedeni, onay açıklaması vs.)
    /// </summary>
    public string? ApprovalNotes { get; set; }

    /// <summary>
    /// Onay durumu (Pending, Approved, Rejected)
    /// </summary>
    public string ApprovalStatus { get; protected set; }

    /// <summary>
    /// Yeni approvable entity oluşturur
    /// </summary>
    protected ApprovableBaseEntity() : base()
    {
        IsApproved = false;
        ApprovalStatus = "Pending";
    }

    /// <summary>
    /// Varolan bir approvable entity'yi yüklerken kullanılır
    /// </summary>
    protected ApprovableBaseEntity(string? id, DateTime createdAt, DateTime? modifiedAt) 
        : base(id, createdAt, modifiedAt)
    {
        IsApproved = false;
        ApprovalStatus = "Pending";
    }

    /// <summary>
    /// Entity'yi onaylar
    /// </summary>
    /// <param name="approvedBy">Onaylayan kullanıcının ID'si</param>
    /// <param name="notes">Onay notları (opsiyonel)</param>
    public virtual void Approve(string approvedBy, string? notes = null)
    {
        IsApproved = true;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        ApprovalNotes = notes;
        ApprovalStatus = "Approved";
        SetModifiedDate();
    }

    /// <summary>
    /// Entity'yi reddeder
    /// </summary>
    /// <param name="rejectedBy">Reddeden kullanıcının ID'si</param>
    /// <param name="reason">Red nedeni</param>
    public virtual void Reject(string rejectedBy, string reason)
    {
        IsApproved = false;
        ApprovedBy = rejectedBy;
        ApprovedAt = DateTime.UtcNow;
        ApprovalNotes = reason;
        ApprovalStatus = "Rejected";
        SetModifiedDate();
    }

    /// <summary>
    /// Onay durumunu beklemede olarak sıfırlar
    /// </summary>
    public virtual void ResetApproval()
    {
        IsApproved = false;
        ApprovedBy = null;
        ApprovedAt = null;
        ApprovalNotes = null;
        ApprovalStatus = "Pending";
        SetModifiedDate();
    }
}
