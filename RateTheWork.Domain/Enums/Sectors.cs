namespace RateTheWork.Domain.Enums;

/// <summary>
/// Sektörler
/// </summary>
public static class Sectors
{
    public const string Technology = "Teknoloji";
    public const string Finance = "Finans";
    public const string Healthcare = "Sağlık";
    public const string Education = "Eğitim";
    public const string Retail = "Perakende";
    public const string Manufacturing = "Üretim";
    public const string Construction = "İnşaat";
    public const string Tourism = "Turizm";
    public const string Logistics = "Lojistik";
    public const string Media = "Medya";
    public const string Consulting = "Danışmanlık";
    public const string RealEstate = "Gayrimenkul";
    public const string Energy = "Enerji";
    public const string Automotive = "Otomotiv";
    public const string Food = "Gıda";
    public const string Other = "Diğer";
    
    public static List<string> GetAll()
    {
        return new List<string>
        {
            Technology, Finance, Healthcare, Education, Retail,
            Manufacturing, Construction, Tourism, Logistics, Media,
            Consulting, RealEstate, Energy, Automotive, Food, Other
        };
    }
}
