using Microsoft.EntityFrameworkCore;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.VerificationRequest;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

/// <summary>
/// Doğrulama talepleri için repository implementasyonu
/// </summary>
public class VerificationRequestRepository : BaseRepository<VerificationRequest>, IVerificationRequestRepository
{
    public VerificationRequestRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Kullanıcının doğrulama taleplerini getirir
    /// </summary>
    public async Task<List<VerificationRequest>> GetRequestsByUserAsync(string userId)
    {
        return await _context.VerificationRequests
            .Where(vr => vr.UserId == userId)
            .OrderByDescending(vr => vr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// İncelemeye ait doğrulama taleplerini getirir
    /// </summary>
    public async Task<List<VerificationRequest>> GetRequestsByReviewAsync(string reviewId)
    {
        return await _context.VerificationRequests
            .Where(vr => vr.ReviewId == reviewId)
            .OrderByDescending(vr => vr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// İnceleme için aktif doğrulama talebini getirir
    /// </summary>
    public async Task<VerificationRequest?> GetActiveRequestByReviewAsync(string reviewId)
    {
        return await _context.VerificationRequests
            .Where(vr => vr.ReviewId == reviewId &&
                         (vr.Status == VerificationRequestStatus.Pending ||
                          vr.Status == VerificationRequestStatus.Processing))
            .OrderByDescending(vr => vr.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Bekleyen doğrulama taleplerini getirir
    /// </summary>
    public async Task<List<VerificationRequest>> GetPendingRequestsAsync()
    {
        return await _context.VerificationRequests
            .Where(vr => vr.Status == VerificationRequestStatus.Pending)
            // .Include(vr => vr.User) // Navigation property not available
            .OrderBy(vr => vr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Doğrulama tipine göre talepleri getirir
    /// </summary>
    public async Task<IEnumerable<VerificationRequest>> GetByTypeAsync(string verificationType)
    {
        return await _context.VerificationRequests
            .Where(vr => vr.Type.ToString() == verificationType)
            // .Include(vr => vr.User) // Navigation property not available
            .OrderByDescending(vr => vr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Duruma göre doğrulama taleplerini getirir
    /// </summary>
    public async Task<IEnumerable<VerificationRequest>> GetByStatusAsync(string status)
    {
        return await _context.VerificationRequests
            .Where(vr => vr.Status.ToString() == status)
            // .Include(vr => vr.User) // Navigation property not available
            .OrderByDescending(vr => vr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Kullanıcının aktif doğrulama talebini getirir
    /// </summary>
    public async Task<VerificationRequest?> GetActiveRequestAsync(string userId, string verificationType)
    {
        return await _context.VerificationRequests
            .Where(vr => vr.UserId == userId &&
                         vr.Type.ToString() == verificationType &&
                         (vr.Status == VerificationRequestStatus.Pending ||
                          vr.Status == VerificationRequestStatus.Processing))
            .OrderByDescending(vr => vr.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Doğrulama koduna göre talebi getirir
    /// </summary>
    public async Task<VerificationRequest?> GetByCodeAsync(string verificationCode)
    {
        return await _context.VerificationRequests
            // .Include(vr => vr.User) // Navigation property not available
            .FirstOrDefaultAsync(vr => vr.VerificationCode == verificationCode);
    }

    /// <summary>
    /// Süresi dolmuş talepleri getirir
    /// </summary>
    public async Task<IEnumerable<VerificationRequest>> GetExpiredRequestsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.VerificationRequests
            .Where(vr => vr.ExpiresAt < now && vr.Status == VerificationRequestStatus.Pending)
            .ToListAsync();
    }
}
