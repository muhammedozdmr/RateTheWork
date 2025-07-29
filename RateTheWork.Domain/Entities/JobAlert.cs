using RateTheWork.Domain.Common;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// İş uyarıları entity'si - Application katmanı uyumluluğu için
/// </summary>
public class JobAlert : BaseEntity
{
    private JobAlert() : base() { }
    
    public string UserId { get; private set; } = string.Empty;
    public string Keywords { get; private set; } = string.Empty;
    public string? Location { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? LastNotifiedAt { get; private set; }
    public string? City { get; private set; }
    public string? JobType { get; private set; }
    
    public static JobAlert Create(string userId, string keywords, string? location = null)
    {
        return new JobAlert
        {
            UserId = userId,
            Keywords = keywords,
            Location = location
        };
    }
    
    public void Deactivate()
    {
        IsActive = false;
        SetModifiedDate();
    }
}