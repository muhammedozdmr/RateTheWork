using RateTheWork.Domain.Entities;
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
    public async Task<IEnumerable<VerificationRequest>> GetByUserIdAsync(Guid userId)
    {
        return await _context.VerificationRequests
            .Where(vr => vr.UserId == userId)
            .OrderByDescending(vr => vr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Bekleyen doğrulama taleplerini getirir
    /// </summary>
    public async Task<IEnumerable<VerificationRequest>> GetPendingRequestsAsync()
    {
        return await _context.VerificationRequests
            .Where(vr => vr.Status == "Pending")
            .Include(vr => vr.User)
            .OrderBy(vr => vr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Doğrulama tipine göre talepleri getirir
    /// </summary>
    public async Task<IEnumerable<VerificationRequest>> GetByTypeAsync(string verificationType)
    {
        return await _context.VerificationRequests
            .Where(vr => vr.Type == verificationType)
            .Include(vr => vr.User)
            .OrderByDescending(vr => vr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Duruma göre doğrulama taleplerini getirir
    /// </summary>
    public async Task<IEnumerable<VerificationRequest>> GetByStatusAsync(string status)
    {
        return await _context.VerificationRequests
            .Where(vr => vr.Status == status)
            .Include(vr => vr.User)
            .OrderByDescending(vr => vr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Kullanıcının aktif doğrulama talebini getirir
    /// </summary>
    public async Task<VerificationRequest?> GetActiveRequestAsync(Guid userId, string verificationType)
    {
        return await _context.VerificationRequests
            .Where(vr => vr.UserId == userId &&
                         vr.Type == verificationType &&
                         (vr.Status == "Pending" || vr.Status == "InProgress"))
            .OrderByDescending(vr => vr.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Doğrulama koduna göre talebi getirir
    /// </summary>
    public async Task<VerificationRequest?> GetByCodeAsync(string verificationCode)
    {
        return await _context.VerificationRequests
            .Include(vr => vr.User)
            .FirstOrDefaultAsync(vr => vr.VerificationCode == verificationCode);
    }

    /// <summary>
    /// Süresi dolmuş talepleri getirir
    /// </summary>
    public async Task<IEnumerable<VerificationRequest>> GetExpiredRequestsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.VerificationRequests
            .Where(vr => vr.ExpiresAt < now && vr.Status == "Pending")
            .ToListAsync();
    }
}
