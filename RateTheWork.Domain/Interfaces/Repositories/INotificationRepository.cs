using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces.Repositories;

/// <summary>
/// Bildirim repository interface'i
/// </summary>
public interface INotificationRepository : IRepository<Notification>
{
    /// <summary>
    /// Kullanıcının okunmamış bildirimlerini getirir
    /// </summary>
    Task<IReadOnlyList<Notification>> GetUnreadNotificationsByUserIdAsync(string userId);

    /// <summary>
    /// Kullanıcının bildirimlerini sayfalı olarak getirir
    /// </summary>
    Task<(IReadOnlyList<Notification> items, int totalCount)> GetUserNotificationsPagedAsync
    (
        string userId
        , int pageNumber
        , int pageSize
        , bool? isRead = null
    );

    /// <summary>
    /// Kullanıcının okunmamış bildirim sayısını getirir
    /// </summary>
    Task<int> GetUnreadCountByUserIdAsync(string userId);

    /// <summary>
    /// Belirli tipteki bildirimleri getirir
    /// </summary>
    Task<IReadOnlyList<Notification>> GetNotificationsByTypeAsync(string userId, string notificationType);

    /// <summary>
    /// Toplu bildirim oluşturur
    /// </summary>
    Task CreateBulkNotificationsAsync(IEnumerable<Notification> notifications);

    /// <summary>
    /// Kullanıcının tüm bildirimlerini okundu olarak işaretler
    /// </summary>
    Task MarkAllAsReadAsync(string userId);
}
