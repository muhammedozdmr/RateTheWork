namespace RateTheWork.Domain.Common;

/// <summary>
/// Onay mekanizması gerektiren entity'ler için temel sınıf.
/// Admin onayı bekleyen şirket, yorum vb. entity'ler bu sınıftan türer.
/// </summary>
public abstract class ApprovableBaseEntity : AuditableBaseEntity
{
    /// <summary>
    /// Onay durumu
    /// </summary>
    public bool IsApproved { get; protected set; }
    
    /// <summary>
    /// Onay durumu metni (Pending, Approved, Rejected)
    /// </summary>
    public string ApprovalStatus { get; protected set; }
    
    /// <summary>
    /// Onaylayan kullanıcının ID'si
    /// </summary>
    public string? ApprovedBy { get; protected set; }
    
    /// <summary>
    /// Onay tarihi
    /// </summary>
    public DateTime? ApprovedAt { get; protected set; }
    
    /// <summary>
    /// Onay notları
    /// </summary>
    public string? ApprovalNotes { get; protected set; }
    
    /// <summary>
    /// Red eden kullanıcının ID'si
    /// </summary>
    public string? RejectedBy { get; protected set; }
    
    /// <summary>
    /// Red tarihi
    /// </summary>
    public DateTime? RejectedAt { get; protected set; }
    
    /// <summary>
    /// Red nedeni
    /// </summary>
    public string? RejectionReason { get; protected set; }

    /// <summary>
    /// Yeni ApprovableBaseEntity oluşturur
    /// </summary>
    protected ApprovableBaseEntity() : base()
    {
        IsApproved = false;
        ApprovalStatus = "Pending";
    }

    /// <summary>
    /// Varolan bir entity'yi yüklerken kullanılır
    /// </summary>
    protected ApprovableBaseEntity(string id, DateTime createdAt, DateTime? modifiedAt, 
        string? createdBy, string? modifiedBy, DateTime? deletedAt, string? deletedBy, bool isDeleted,
        bool isApproved, string approvalStatus, string? approvedBy, DateTime? approvedAt, 
        string? approvalNotes, string? rejectedBy, DateTime? rejectedAt, string? rejectionReason) 
        : base(id, createdAt, modifiedAt, createdBy, modifiedBy, deletedAt, deletedBy, isDeleted)
    {
        IsApproved = isApproved;
        ApprovalStatus = approvalStatus;
        ApprovedBy = approvedBy;
        ApprovedAt = approvedAt;
        ApprovalNotes = approvalNotes;
        RejectedBy = rejectedBy;
        RejectedAt = rejectedAt;
        RejectionReason = rejectionReason;
    }

    /// <summary>
    /// Entity'yi onayla
    /// </summary>
    public virtual void Approve(string approvedBy, string? notes = null)
    {
        IsApproved = true;
        ApprovalStatus = "Approved";
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        ApprovalNotes = notes;
        SetModifiedAudit(approvedBy);
    }

    /// <summary>
    /// Entity'yi reddet
    /// </summary>
    public virtual void Reject(string rejectedBy, string reason)
    {
        IsApproved = false;
        ApprovalStatus = "Rejected";
        RejectedBy = rejectedBy;
        RejectedAt = DateTime.UtcNow;
        RejectionReason = reason;
        SetModifiedAudit(rejectedBy);
    }

    /// <summary>
    /// Onay durumunu sıfırla (tekrar değerlendirme için)
    /// </summary>
    public virtual void ResetApproval()
    {
        IsApproved = false;
        ApprovalStatus = "Pending";
        ApprovedBy = null;
        ApprovedAt = null;
        ApprovalNotes = null;
        RejectedBy = null;
        RejectedAt = null;
        RejectionReason = null;
        SetModifiedDate();
    }
}
