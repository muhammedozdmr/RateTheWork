namespace RateTheWork.Domain.Enums.Review;

/// <summary>
/// Yorum tipleri
/// </summary>
public static class CommentTypes
{
    public const string Overall = "Overall";
    public const string WorkEnvironment = "WorkEnvironment";
    public const string Management = "Management";
    public const string CareerGrowth = "CareerGrowth";
    public const string WorkLifeBalance = "WorkLifeBalance";
    public const string Benefits = "Benefits";
    public const string Salary = "Salary";
    public const string Culture = "Culture";
    public const string Training = "Training";
    public const string Technology = "Technology";

    /// <summary>
    /// Tüm yorum tipleri
    /// </summary>
    public static readonly List<string> All = new()
    {
        Overall,
        WorkEnvironment,
        Management,
        CareerGrowth,
        WorkLifeBalance,
        Benefits,
        Salary,
        Culture,
        Training,
        Technology
    };

    /// <summary>
    /// Yorum tipi geçerli mi kontrolü
    /// </summary>
    public static bool IsValid(string commentType)
    {
        return !string.IsNullOrWhiteSpace(commentType) && All.Contains(commentType);
    }

    /// <summary>
    /// Yorum tipinin görünen adını döndürür
    /// </summary>
    public static string GetDisplayName(string commentType)
    {
        return commentType switch
        {
            Overall => "Genel Değerlendirme",
            WorkEnvironment => "Çalışma Ortamı",
            Management => "Yönetim",
            CareerGrowth => "Kariyer Gelişimi",
            WorkLifeBalance => "İş-Yaşam Dengesi",
            Benefits => "Yan Haklar",
            Salary => "Maaş",
            Culture => "Şirket Kültürü",
            Training => "Eğitim ve Gelişim",
            Technology => "Teknoloji ve Araçlar",
            _ => commentType
        };
    }
}