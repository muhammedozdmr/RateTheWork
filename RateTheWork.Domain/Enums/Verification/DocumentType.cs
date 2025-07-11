namespace RateTheWork.Domain.Enums.Verification;

/// <summary>
/// Belge türleri
/// </summary>
public enum DocumentType
{
    /// <summary>
    /// Maaş bordrosu
    /// </summary>
    PaySlip,
    
    /// <summary>
    /// İş sözleşmesi
    /// </summary>
    EmploymentContract,
    
    /// <summary>
    /// Çalışma belgesi
    /// </summary>
    EmploymentCertificate,
    
    /// <summary>
    /// Şirket kimlik kartı
    /// </summary>
    CompanyIdCard,
    
    /// <summary>
    /// İşten ayrılma yazısı
    /// </summary>
    SeveranceLetter,
    
    /// <summary>
    /// Vergi beyannamesi
    /// </summary>
    TaxReturn,
    
    /// <summary>
    /// Ticaret sicil gazetesi
    /// </summary>
    TradeRegistry,
    
    /// <summary>
    /// Diğer
    /// </summary>
    Other
}
