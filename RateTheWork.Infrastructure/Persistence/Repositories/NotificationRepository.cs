using Microsoft.EntityFrameworkCore;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

/// <summary>
/// Bildirim yönetimi için repository implementasyonu
/// </summary>
public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Kullanıcının okunmamış bildirimlerini getirir
    /// </summary>
    public async Task<IReadOnlyList<Notification>> GetUnreadNotificationsByUserIdAsync(string userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Kullanıcının bildirimlerini sayfalı olarak getirir
    /// </summary>
    public async Task<(IReadOnlyList<Notification> items, int totalCount)> GetUserNotificationsPagedAsync
    (
        string userId
        , int pageNumber
        , int pageSize
        , bool? isRead = null
    )
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId);

        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// Kullanıcının okunmamış bildirim sayısını getirir
    /// </summary>
    public async Task<int> GetUnreadCountByUserIdAsync(string userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    /// <summary>
    /// Belirli tipteki bildirimleri getirir
    /// </summary>
    public async Task<IReadOnlyList<Notification>> GetNotificationsByTypeAsync(string userId, string notificationType)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && n.TypeString == notificationType)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Toplu bildirim oluşturur
    /// </summary>
    public async Task CreateBulkNotificationsAsync(IEnumerable<Notification> notifications)
    {
        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Kullanıcının tüm bildirimlerini okundu olarak işaretler
    /// </summary>
    public async Task MarkAllAsReadAsync(string userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.MarkAsRead();
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Bildirimleri okundu olarak işaretler
    /// </summary>
    public async Task MarkAsReadAsync(string userId, List<string> notificationIds)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && notificationIds.Contains(n.Id))
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.MarkAsRead();
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Eski bildirimleri siler
    /// </summary>
    public async Task DeleteOldNotificationsAsync(int daysToKeep)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

        var oldNotifications = await _context.Notifications
            .Where(n => n.CreatedAt < cutoffDate && n.IsRead)
            .ToListAsync();

        _context.Notifications.RemoveRange(oldNotifications);
        await _context.SaveChangesAsync();
    }
}
