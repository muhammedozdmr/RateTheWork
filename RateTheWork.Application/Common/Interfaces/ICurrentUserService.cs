namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// Şu anki oturum açmış kullanıcı bilgilerini sağlayan servis.
/// JWT token'dan veya session'dan kullanıcı bilgilerini okur.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Oturum açmış kullanıcının ID'si
    /// </summary>
    string? UserId { get; }
    
    /// <summary>
    /// Kullanıcının email adresi
    /// </summary>
    string? Email { get; }
    
    /// <summary>
    /// Kullanıcının anonim kullanıcı adı
    /// </summary>
    string? AnonymousUsername { get; }
    
    /// <summary>
    /// Kullanıcının adı (AnonymousUsername veya FirstName LastName)
    /// </summary>
    string? UserName { get; }
    
    /// <summary>
    /// Kullanıcı oturum açmış mı?
    /// </summary>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// Kullanıcının rolleri (ileride admin paneli için)
    /// </summary>
    List<string> Roles { get; }
    
    /// <summary>
    /// Kullanıcının IP adresi
    /// </summary>
    string? IpAddress { get; }
}
