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
    /// Kullanıcının bildirimlerini getirir
    /// </summary>
    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, bool includeRead = false)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId);

        if (!includeRead)
            query = query.Where(n => !n.IsRead);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Okunmamış bildirim sayısını getirir
    /// </summary>
    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    /// <summary>
    /// Bildirim tipine göre bildirimleri getirir
    /// </summary>
    public async Task<IEnumerable<Notification>> GetByTypeAsync(Guid userId, string type)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && n.Type == type)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Bildirimleri okundu olarak işaretler
    /// </summary>
    public async Task MarkAsReadAsync(Guid userId, List<Guid> notificationIds)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && notificationIds.Contains(n.Id))
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Tüm bildirimleri okundu olarak işaretler
    /// </summary>
    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
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
