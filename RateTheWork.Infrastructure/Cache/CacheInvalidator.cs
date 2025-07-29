using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;

namespace RateTheWork.Infrastructure.Cache;

/// <summary>
/// Önbellek geçersizleştirme servisi
/// İlişkili önbellekleri temizlemek için kullanılır
/// </summary>
public class CacheInvalidator
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheInvalidator> _logger;

    public CacheInvalidator
    (
        ICacheService cacheService
        , ILogger<CacheInvalidator> logger
    )
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Kullanıcı önbelleğini geçersizleştirir
    /// </summary>
    public async Task InvalidateUserCacheAsync(Guid userId)
    {
        var keys = new[]
        {
            CachePolicies.GetUserKey(userId), CachePolicies.GetListKey("user", "active")
            , CachePolicies.GetListKey("user", "company", userId)
        };

        await InvalidateKeysAsync(keys);
        _logger.LogDebug("Kullanıcı önbelleği geçersizleştirildi: {UserId}", userId);
    }

    /// <summary>
    /// Şirket önbelleğini geçersizleştirir
    /// </summary>
    public async Task InvalidateCompanyCacheAsync(Guid companyId)
    {
        var keys = new[]
        {
            CachePolicies.GetCompanyKey(companyId), CachePolicies.GetListKey("company", "active")
            , CachePolicies.GetListKey("company", "verified"), CachePolicies.GetListKey("branch", "company", companyId)
        };

        await InvalidateKeysAsync(keys);
        _logger.LogDebug("Şirket önbelleği geçersizleştirildi: {CompanyId}", companyId);
    }

    /// <summary>
    /// İş ilanı önbelleğini geçersizleştirir
    /// </summary>
    public async Task InvalidateJobPostingCacheAsync(Guid jobPostingId, Guid companyId)
    {
        var keys = new[]
        {
            CachePolicies.GetJobPostingKey(jobPostingId), CachePolicies.GetListKey("jobposting", "active")
            , CachePolicies.GetListKey("jobposting", "company", companyId)
            , CachePolicies.GetListKey("jobposting", "recent")
        };

        await InvalidateKeysAsync(keys);
        _logger.LogDebug("İş ilanı önbelleği geçersizleştirildi: {JobPostingId}", jobPostingId);
    }

    /// <summary>
    /// İnceleme önbelleğini geçersizleştirir
    /// </summary>
    public async Task InvalidateReviewCacheAsync(Guid reviewId, Guid companyId, Guid userId)
    {
        var keys = new[]
        {
            CachePolicies.GetReviewKey(reviewId), CachePolicies.GetListKey("review", "company", companyId)
            , CachePolicies.GetListKey("review", "user", userId), CachePolicies.GetListKey("review", "recent"),
            // Şirket ortalama puanı da güncellenmeli
            $"company:rating:{companyId}"
        };

        await InvalidateKeysAsync(keys);
        _logger.LogDebug("İnceleme önbelleği geçersizleştirildi: {ReviewId}", reviewId);
    }

    /// <summary>
    /// Şube önbelleğini geçersizleştirir
    /// </summary>
    public async Task InvalidateBranchCacheAsync(Guid branchId, Guid companyId)
    {
        var keys = new[]
        {
            CachePolicies.GetBranchKey(branchId), CachePolicies.GetListKey("branch", "company", companyId)
            , CachePolicies.GetListKey("branch", "active")
        };

        await InvalidateKeysAsync(keys);
        _logger.LogDebug("Şube önbelleği geçersizleştirildi: {BranchId}", branchId);
    }

    /// <summary>
    /// Tüm önbelleği temizler (dikkatli kullanın!)
    /// </summary>
    public async Task InvalidateAllCacheAsync()
    {
        await _cacheService.ClearAsync();
        _logger.LogWarning("TÜM önbellek temizlendi!");
    }

    /// <summary>
    /// Tag bazlı önbellek temizleme
    /// </summary>
    public async Task InvalidateByTagAsync(string tag)
    {
        // Not: Bu özellik cache provider'a göre değişebilir
        // Redis için tag bazlı temizleme eklenebilir
        _logger.LogDebug("Tag bazlı önbellek temizleme: {Tag}", tag);
    }

    /// <summary>
    /// Belirtilen anahtarları geçersizleştirir
    /// </summary>
    private async Task InvalidateKeysAsync(string[] keys)
    {
        foreach (var key in keys)
        {
            await _cacheService.RemoveAsync(key);
        }
    }

    /// <summary>
    /// Pattern bazlı önbellek temizleme
    /// </summary>
    public async Task InvalidateByPatternAsync(string pattern)
    {
        // Not: Bu özellik Redis gibi provider'larda kullanılabilir
        // Örnek: "user:*" tüm kullanıcı önbelleklerini temizler
        _logger.LogDebug("Pattern bazlı önbellek temizleme: {Pattern}", pattern);
    }
}
