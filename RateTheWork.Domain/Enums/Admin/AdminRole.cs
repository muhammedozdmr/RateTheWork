namespace RateTheWork.Domain.Enums;

/// <summary>
/// Admin rolleri
/// </summary>
public enum AdminRole
{
    /// <summary>
    /// Süper admin - Tüm yetkiler
    /// </summary>
    SuperAdmin,
    
    /// <summary>
    /// Admin - Yönetici yetkiler
    /// </summary>
    Admin,
    
    /// <summary>
    /// Moderatör - İçerik yönetimi
    /// </summary>
    Moderator,
    
    /// <summary>
    /// İçerik yöneticisi - Sadece içerik onaylama
    /// </summary>
    ContentManager,
    
    /// <summary>
    /// Destek - Sadece okuma ve rapor alma
    /// </summary>
    Support
}

