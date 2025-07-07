namespace RateTheWork.Application.Common.Mappings.DTOs;

/// <summary>
/// Yorum listesi için özet DTO
/// </summary>
public record ReviewDto
{
    /// <summary>
    /// Yorum ID'si
    /// </summary>
    public string ReviewId { get; init; } = string.Empty;
    
    /// <summary>
    /// Yorumu yapan kullanıcının anonim adı
    /// </summary>
    public string AuthorUsername { get; init; } = string.Empty;
    
    /// <summary>
    /// Yorum yapılan şirket adı
    /// </summary>
    public string CompanyName { get; init; } = string.Empty;
    
    /// <summary>
    /// Yorum türü (Maaş, Çalışma Ortamı vb.)
    /// </summary>
    public string CommentType { get; init; } = string.Empty;
    
    /// <summary>
    /// Verilen puan (1-5)
    /// </summary>
    public decimal OverallRating { get; init; }
    
    /// <summary>
    /// Yorum metni (ilk 200 karakter)
    /// </summary>
    public string? CommentPreview { get; init; } = string.Empty;
    
    /// <summary>
    /// Yorum tarihi
    /// </summary>
    public DateTime PostedDate { get; init; }
    
    /// <summary>
    /// Upvote sayısı
    /// </summary>
    public int Upvotes { get; init; }
    
    /// <summary>
    /// Downvote sayısı
    /// </summary>
    public int Downvotes { get; init; }
    
    /// <summary>
    /// Net oy (upvotes - downvotes)
    /// </summary>
    public int NetVotes { get; init; }
    
    /// <summary>
    /// Belge ile doğrulandı mı?
    /// </summary>
    public bool IsVerified { get; init; }
}

