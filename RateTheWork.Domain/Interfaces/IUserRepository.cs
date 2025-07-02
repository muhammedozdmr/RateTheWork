using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces;

/// <summary>
/// Kullanıcı entity'si için repository interface'i.
/// Kullanıcıya özel veritabanı işlemlerini tanımlar.
/// </summary>
public interface IUserRepository : IBaseRepository<User>
{
    /// <summary>
    /// Email adresine göre kullanıcı arar
    /// </summary>
    /// <param name="email">Aranacak email adresi</param>
    /// <returns>Bulunan kullanıcı veya null</returns>
    Task<User?> GetByEmailAsync(string email);
    
    /// <summary>
    /// Anonim kullanıcı adına göre kullanıcı arar
    /// </summary>
    /// <param name="username">Aranacak kullanıcı adı</param>
    /// <returns>Bulunan kullanıcı veya null</returns>
    Task<User?> GetByAnonymousUsernameAsync(string username);
    
    /// <summary>
    /// TC Kimlik numarasına göre kullanıcı arar (şifrelenmiş veri üzerinden)
    /// </summary>
    /// <param name="tcIdentity">Aranacak TC Kimlik numarası</param>
    /// <returns>Bulunan kullanıcı veya null</returns>
    Task<User?> GetByTcIdentityAsync(string tcIdentity);
    
    /// <summary>
    /// Kullanıcının yaptığı yorumları sayfalı olarak getirir
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="page">Sayfa numarası (1'den başlar)</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>Kullanıcının yorumları</returns>
    Task<List<Review>> GetUserReviewsAsync(string userId, int page = 1, int pageSize = 10);
    
    /// <summary>
    /// Kullanıcının kazandığı rozetleri getirir
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <returns>Kullanıcının rozetleri</returns>
    Task<List<UserBadge>> GetUserBadgesAsync(string userId);
    
    /// <summary>
    /// Kullanıcının bildirimlerini getirir
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="unreadOnly">Sadece okunmamış bildirimleri getir</param>
    /// <returns>Kullanıcının bildirimleri</returns>
    Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
    
    /// <summary>
    /// Kullanıcının belirli bir şirkete belirli türde yorum yapıp yapmadığını kontrol eder
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="companyId">Şirket ID'si</param>
    /// <param name="commentType">Yorum türü (örn: "Maaş", "Çalışma Ortamı")</param>
    /// <returns>Yorum yapılmış mı?</returns>
    Task<bool> HasUserReviewedCompanyAsync(string userId, string companyId, string commentType);
    
    /// <summary>
    /// Kullanıcının banlı olup olmadığını kontrol eder
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <returns>Banlı mı?</returns>
    Task<bool> IsUserBannedAsync(string userId);
    
    /// <summary>
    /// Kullanıcının toplam uyarı sayısını getirir
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <returns>Uyarı sayısı</returns>
    Task<int> GetUserWarningCountAsync(string userId);
    
    /// <summary>
    /// Email adresinin başka bir kullanıcı tarafından kullanılıp kullanılmadığını kontrol eder
    /// </summary>
    /// <param name="email">Kontrol edilecek email</param>
    /// <returns>Email kullanımda mı?</returns>
    Task<bool> IsEmailTakenAsync(string email);
    
    /// <summary>
    /// Kullanıcı adının başka bir kullanıcı tarafından kullanılıp kullanılmadığını kontrol eder
    /// </summary>
    /// <param name="username">Kontrol edilecek kullanıcı adı</param>
    /// <returns>Kullanıcı adı kullanımda mı?</returns>
    Task<bool> IsUsernameTakenAsync(string username);
}
