namespace RateTheWork.Domain.Enums;

/// <summary>
/// Yorum türleri
/// </summary>
public static class CommentTypes
{
    public const string SalaryAndBenefits = "Maaş & Yan Haklar";
    public const string WorkEnvironment = "Çalışma Ortamı";
    public const string Management = "Yönetim";
    public const string CareerDevelopment = "Kariyer Gelişimi";
    public const string WorkLifeBalance = "İş-Yaşam Dengesi";
    public const string CompanyCulture = "Şirket Kültürü";
    public const string Interview = "Mülakat Süreci";
    public const string Other = "Diğer";
    
    /// <summary>
    /// Tüm yorum türlerini liste olarak döner
    /// </summary>
    public static List<string> GetAll()
    {
        return new List<string>
        {
            SalaryAndBenefits,
            WorkEnvironment,
            Management,
            CareerDevelopment,
            WorkLifeBalance,
            CompanyCulture,
            Interview,
            Other
        };
    }
    
    /// <summary>
    /// Geçerli bir yorum türü mü kontrol eder
    /// </summary>
    public static bool IsValid(string commentType)
    {
        return GetAll().Contains(commentType);
    }
}
