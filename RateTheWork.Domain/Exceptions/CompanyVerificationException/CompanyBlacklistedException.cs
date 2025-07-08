namespace RateTheWork.Domain.Exceptions.CompanyVerificationException;

/// <summary>
/// Şirket kara listede exception'ı
/// </summary>
public class CompanyBlacklistedException : DomainException
{
    public Guid CompanyId { get; }
    public string Reason { get; }
    public DateTime BlacklistedAt { get; }

    public CompanyBlacklistedException(Guid companyId, string reason, DateTime blacklistedAt)
        : base($"Company is blacklisted. Reason: {reason}")
    {
        CompanyId = companyId;
        Reason = reason;
        BlacklistedAt = blacklistedAt;
    }
}
