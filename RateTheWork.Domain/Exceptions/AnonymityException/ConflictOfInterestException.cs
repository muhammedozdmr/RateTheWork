namespace RateTheWork.Domain.Exceptions.AnonymityException;

/// <summary>
/// Çıkar çatışması exception'ı (kendi şirketini değerlendirme vb.)
/// </summary>
public class ConflictOfInterestException : DomainException
{
    public string ConflictType { get; }
    public Guid UserId { get; }
    public Guid CompanyId { get; }

    public ConflictOfInterestException(string conflictType, Guid userId, Guid companyId)
        : base($"Conflict of interest detected: {conflictType}. User cannot review this company.")
    {
        ConflictType = conflictType;
        UserId = userId;
        CompanyId = companyId;
    }
}

