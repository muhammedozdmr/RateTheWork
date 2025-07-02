namespace RateTheWork.Domain.Entities.Common;

// Onay gerektiren entity'ler için (Company, Review vs.)
public abstract class ApprovableBaseEntity : AuditableBaseEntity
{
    public bool IsApproved { get; set; } = false;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }
}
