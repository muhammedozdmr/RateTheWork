using RateTheWork.Domain.Common;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Şirket entity'si - Platformdaki şirket bilgilerini temsil eder.
/// ApprovableBaseEntity'den türer ve admin onayı gerektirir.
/// </summary>
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

    public decimal AverageRating { get; private set; } // Şirketin ortalama puanı (yorum puanlarından hesaplanır)
    public int TotalReviews { get; private set; } // Toplam yorum sayısı

    /// <summary>
    /// Yeni şirket oluşturur
    /// </summary>
    public Company(
        string name, 
        string taxId, 
        string mersisNo, 
        string sector, 
        string address, 
        string phoneNumber, 
        string email, 
        string websiteUrl
    ) : base()
    {
        Name = name;
        TaxId = taxId;
        MersisNo = mersisNo;
        Sector = sector;
        Address = address;
        PhoneNumber = phoneNumber;
        Email = email;
        WebsiteUrl = websiteUrl;
        AverageRating = 0;
        TotalReviews = 0;
        // ApprovableBaseEntity constructor'ı otomatik çalışacak
        // IsApproved = false; // Varsayılan olarak false
        // ApprovalStatus = "Pending"; // Varsayılan olarak Pending
    }

    /// <summary>
    /// Şirket bilgilerini günceller
    /// </summary>
    public void UpdateInfo(
        string name,
        string sector,
        string address,
        string phoneNumber,
        string email,
        string websiteUrl
    )
    {
        Name = name;
        Sector = sector;
        Address = address;
        PhoneNumber = phoneNumber;
        Email = email;
        WebsiteUrl = websiteUrl;
        SetModifiedDate();
    }

    /// <summary>
    /// Sosyal medya linklerini günceller
    /// </summary>
    public void UpdateSocialMediaLinks(
        string? linkedInUrl = null,
        string? xUrl = null,
        string? instagramUrl = null,
        string? facebookUrl = null
    )
    {
        if (linkedInUrl != null) LinkedInUrl = linkedInUrl;
        if (xUrl != null) XUrl = xUrl;
        if (instagramUrl != null) InstagramUrl = instagramUrl;
        if (facebookUrl != null) FacebookUrl = facebookUrl;
        SetModifiedDate();
    }

    /// <summary>
    /// Logo URL'ini günceller
    /// </summary>
    public void UpdateLogo(string logoUrl)
    {
        LogoUrl = logoUrl;
        SetModifiedDate();
    }

    /// <summary>
    /// Yorum istatistiklerini günceller (domain service tarafından çağrılır)
    /// </summary>
    public void UpdateReviewStatistics(decimal averageRating, int totalReviews)
    {
        AverageRating = averageRating;
        TotalReviews = totalReviews;
        SetModifiedDate();
    }

    /// <summary>
    /// Yeni yorum eklendiğinde çağrılır
    /// </summary>
    public void AddReview(decimal rating)
    {
        var totalRating = AverageRating * TotalReviews + rating;
        TotalReviews++;
        AverageRating = Math.Round(totalRating / TotalReviews, 2);
        SetModifiedDate();
    }

    /// <summary>
    /// Yorum silindiğinde çağrılır
    /// </summary>
    public void RemoveReview(decimal rating)
    {
        if (TotalReviews <= 1)
        {
            AverageRating = 0;
            TotalReviews = 0;
        }
        else
        {
            var totalRating = AverageRating * TotalReviews - rating;
            TotalReviews--;
            AverageRating = Math.Round(totalRating / TotalReviews, 2);
        }
        SetModifiedDate();
    }

    /// <summary>
    /// Onay gerektiğinde override edilir
    /// </summary>
    public override void Approve(string approvedBy, string? notes = null)
    {
        base.Approve(approvedBy, notes);
        // Şirket onaylandığında yapılacak ek işlemler
        // Örn: Email gönderme, bildirim oluşturma vb. için event
    }

    /// <summary>
    /// Red edildiğinde override edilir
    /// </summary>
    public override void Reject(string rejectedBy, string reason)
    {
        base.Reject(rejectedBy, reason);
        // Şirket reddedildiğinde yapılacak ek işlemler
    }
}
