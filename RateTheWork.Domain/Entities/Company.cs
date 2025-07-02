using RateTheWork.Domain.Entities.Base;

namespace RateTheWork.Domain.Entities;

// Şirket: Platformdaki şirket bilgilerini temsil eder.
public class Company : ApprovableBaseEntity
{
    public string Name { get; set; } // Şirket Tam Adı (örn: "ABC Teknoloji A.Ş.")
    public string TaxId { get; set; } // Vergi Kimlik Numarası (VKN) - Yasal bilgi
    public string MersisNo { get; set; } // MERSİS Numarası - Yasal bilgi
    public string Sector { get; set; } // Şirketin faaliyet gösterdiği sektör (örn: "Yazılım", "Finans")
    public string Address { get; set; } // Şirket adresi
    public string PhoneNumber { get; set; } // Şirket iletişim telefon numarası
    public string Email { get; set; } // Şirket iletişim e-posta adresi
    public string WebsiteUrl { get; set; } // Şirket web sitesi URL'si
    public string? LogoUrl { get; set; } // Şirket logosunun URL'si (isteğe bağlı)

    // Sosyal Medya Linkleri (Her bir platform için ayrı)
    public string? LinkedInUrl { get; set; }
    public string? XUrl { get; set; } // Eski adıyla Twitter
    public string? InstagramUrl { get; set; }
    public string? FacebookUrl { get; set; }
    // Diğer sosyal medya linkleri eklenebilir

    public decimal AverageRating { get; set; } // Şirketin ortalama puanı (yorum puanlarından hesaplanır)
    public int TotalReviews { get; set; } // Toplam yorum sayısı

    public Company(string name, string taxId, string mersisNo, string sector, string address, string phoneNumber, string email, string websiteUrl)
    {
        Name = name;
        TaxId = taxId;
        MersisNo = mersisNo;
        Sector = sector;
        Address = address;
        PhoneNumber = phoneNumber;
        Email = email;
        WebsiteUrl = websiteUrl;
        // BaseEntity constructor'ı otomatik çalışacak
        // IsApproved = false; // ApprovableBaseEntity'de varsayılan olarak false
    }
}
