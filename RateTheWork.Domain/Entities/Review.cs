using RateTheWork.Domain.Entities.Common;

namespace RateTheWork.Domain.Entities;

// Yorum: Bir şirkete yapılan yorumu ve puanlamayı temsil eder.
public class Review : AuditableBaseEntity
{
    public string CompanyId { get; set; } // Yorumun yapıldığı şirket ID'si
    public string UserId { get; set; } // Yorumu yapan kullanıcı ID'si

    public string CommentType { get; set; } // Yorum türü (Maaş & Yan Haklar, Çalışma Ortamı vb.)
    public decimal OverallRating { get; set; } // Yorumun genel puanı (0.0 - 5.0 arası)
    public string CommentText { get; set; } // Yorum metni
    public string? DocumentUrl { get; set; } // Yüklenen belgenin URL'si (isteğe bağlı)
    public bool IsDocumentVerified { get; set; } = false; // Yorum belgesi admin tarafından onaylandı mı?

    public int Upvotes { get; set; } = 0; // Faydalı oyu sayısı
    public int Downvotes { get; set; } = 0; // Faydalı değil oyu sayısı
    public int ReportCount { get; set; } = 0; // Şikayet sayısı
    public bool IsActive { get; set; } = true; // Yorum aktif mi (silindi/gizlendi mi)

    // Constructor: Zorunlu alanları başlangıçta alarak tutarlı bir Review nesnesi oluşturur.
    public Review(string companyId, string userId, string commentType, decimal overallRating, string commentText)
    {
        CompanyId = companyId;
        UserId = userId;
        CommentType = commentType;
        OverallRating = overallRating;
        CommentText = commentText;
        IsDocumentVerified = false; // Yeni yorumlarda varsayılan olarak belge doğrulanmamış
        // BaseEntity constructor'ı otomatik çalışacak
        // Upvotes, Downvotes, ReportCount, IsActive varsayılan değerleriyle başlar
    }
}
