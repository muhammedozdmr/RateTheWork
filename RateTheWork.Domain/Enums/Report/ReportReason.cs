namespace RateTheWork.Domain.Enums.Report;

/// <summary>
/// Şikayet nedenleri
/// </summary>
public enum ReportReason
{
    /// <summary>
    /// Spam veya yanıltıcı içerik
    /// </summary>
    Spam = 1,
    
    /// <summary>
    /// Hakaret veya küfür içeriyor
    /// </summary>
    OffensiveContent = 2,
    
    /// <summary>
    /// Yanlış veya yanıltıcı bilgi
    /// </summary>
    Misinformation = 3,
    
    /// <summary>
    /// Kişisel bilgi paylaşımı
    /// </summary>
    PersonalInformation = 4,
    
    /// <summary>
    /// Telif hakkı ihlali
    /// </summary>
    CopyrightViolation = 5,
    
    /// <summary>
    /// Nefret söylemi
    /// </summary>
    HateSpeech = 6,
    
    /// <summary>
    /// Taciz veya tehdit
    /// </summary>
    Harassment = 7,
    
    /// <summary>
    /// Reklam veya promosyon
    /// </summary>
    Advertisement = 8,
    
    /// <summary>
    /// Konu dışı içerik
    /// </summary>
    OffTopic = 9,
    
    /// <summary>
    /// Diğer
    /// </summary>
    Other = 10
}

/// <summary>
/// ReportReason yardımcı metotları
/// </summary>
public static class ReportReasonExtensions
{
    /// <summary>
    /// Tüm şikayet nedenlerini döndürür
    /// </summary>
    public static List<ReportReason> GetAll()
    {
        return Enum.GetValues<ReportReason>().ToList();
    }
    
    /// <summary>
    /// Display metni döndürür
    /// </summary>
    public static string GetDisplayText(this ReportReason reason)
    {
        return reason switch
        {
            ReportReason.Spam => "Spam veya yanıltıcı içerik",
            ReportReason.OffensiveContent => "Hakaret veya küfür içeriyor",
            ReportReason.Misinformation => "Yanlış veya yanıltıcı bilgi",
            ReportReason.PersonalInformation => "Kişisel bilgi paylaşımı",
            ReportReason.CopyrightViolation => "Telif hakkı ihlali",
            ReportReason.HateSpeech => "Nefret söylemi",
            ReportReason.Harassment => "Taciz veya tehdit",
            ReportReason.Advertisement => "Reklam veya promosyon",
            ReportReason.OffTopic => "Konu dışı içerik",
            ReportReason.Other => "Diğer",
            _ => reason.ToString()
        };
    }
}