namespace RateTheWork.Application.Common.Mappings.DTOs;

/// <summary>
/// Kullanıcı bilgilerinin basit görünümü.
/// Liste ve özet bilgilerde kullanılır.
/// </summary>
public record UserDto
{
    /// <summary>
    /// Kullanıcı ID'si
    /// </summary>
    public string UserId { get; init; } = string.Empty;
    
    /// <summary>
    /// Anonim kullanıcı adı
    /// </summary>
    public string Username { get; init; } = string.Empty;
    
    /// <summary>
    /// Kullanıcının mesleği
    /// </summary>
    public string Profession { get; init; } = string.Empty;
    
    /// <summary>
    /// Kayıt tarihi
    /// </summary>
    public DateTime RegistrationDate { get; init; }
    
    /// <summary>
    /// Kullanıcının aktif olup olmadığı
    /// </summary>
    public bool IsActive { get; init; }
}


