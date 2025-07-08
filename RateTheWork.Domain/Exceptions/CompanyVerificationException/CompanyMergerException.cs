namespace RateTheWork.Domain.Exceptions.CompanyVerificationException;

/// <summary>
/// Şirket birleşme/satın alma durumu exception'ı
/// </summary>
public class CompanyMergerException : DomainException
{
    public Guid OriginalCompanyId { get; }
    public Guid MergedWithCompanyId { get; }
    public DateTime MergerDate { get; }

    public CompanyMergerException(Guid originalCompanyId, Guid mergedWithCompanyId, DateTime mergerDate)
        : base($"Company has been merged with another company on {mergerDate:yyyy-MM-dd}.")
    {
        OriginalCompanyId = originalCompanyId;
        MergedWithCompanyId = mergedWithCompanyId;
        MergerDate = mergerDate;
    }
}
