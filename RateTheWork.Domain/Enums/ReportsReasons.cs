namespace RateTheWork.Domain.Enums;

/// <summary>
/// Şikayet nedenleri
/// </summary>
public static class ReportReasons
{
    public const string InappropriateContent = "Uygunsuz İçerik";
    public const string FalseInformation = "Yanlış Bilgi";
    public const string Spam = "Spam";
    public const string HateSpeech = "Nefret Söylemi";
    public const string PersonalAttack = "Kişisel Saldırı";
    public const string ConfidentialInfo = "Gizli Bilgi Paylaşımı";
    public const string Other = "Diğer";
    
    public static List<string> GetAll()
    {
        return new List<string>
        {
            InappropriateContent,
            FalseInformation,
            Spam,
            HateSpeech,
            PersonalAttack,
            ConfidentialInfo,
            Other
        };
    }
}
