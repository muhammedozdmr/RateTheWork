namespace RateTheWork.Domain.Enums.Review;

/// <summary>
/// Yorum türleri
/// </summary>
public enum CommentType
{
    /// <summary>
    /// Maaş ve yan haklar hakkında yorum
    /// </summary>
    SalaryAndBenefits,
    
    /// <summary>
    /// Çalışma ortamı hakkında yorum
    /// </summary>
    WorkEnvironment,
    
    /// <summary>
    /// Yönetim hakkında yorum
    /// </summary>
    Management,
    
    /// <summary>
    /// Kariyer gelişimi hakkında yorum
    /// </summary>
    CareerDevelopment,
    
    /// <summary>
    /// İş-yaşam dengesi hakkında yorum
    /// </summary>
    WorkLifeBalance,
    
    /// <summary>
    /// Şirket kültürü hakkında yorum
    /// </summary>
    CompanyCulture,
    
    /// <summary>
    /// Mülakat süreci hakkında yorum
    /// </summary>
    InterviewProcess,
    
    /// <summary>
    /// Diğer konular hakkında yorum
    /// </summary>
    Other
}