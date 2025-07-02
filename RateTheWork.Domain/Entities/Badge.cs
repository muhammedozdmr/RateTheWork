using RateTheWork.Domain.Entities.Common;

namespace RateTheWork.Domain.Entities;

// Rozet: Kazanılabilir rozetlerin tanımlarını tutmak için.
public class Badge : BaseEntity
{
    public string Name { get; set; } // Rozet Adı (örn: "Tecrübeli Yorumcu")
    public string Description { get; set; } // Rozet Açıklaması
    public string IconUrl { get; set; } // Rozet ikonunun URL'si
    public string Criteria { get; set; } // Rozetin kazanılma kriterleri (metin olarak, örn: "Toplam 25'ten fazla yorum")

    public Badge(string name, string description, string iconUrl, string criteria)
    {
        Name = name;
        Description = description;
        IconUrl = iconUrl;
        Criteria = criteria;
        // BaseEntity constructor'ı otomatik çalışacak
    }
}
