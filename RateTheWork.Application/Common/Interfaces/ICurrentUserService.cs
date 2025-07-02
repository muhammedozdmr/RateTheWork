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
    /// Kullanıcı oturum açmış mı?
    /// </summary>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// Kullanıcının rolleri (ileride admin paneli için)
    /// </summary>
    List<string> Roles { get; }
}
