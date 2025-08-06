using Microsoft.Extensions.Logging;
using RateTheWork.Application.Services.Interfaces;
using RateTheWork.Domain.Interfaces.Repositories;
using System.Linq.Expressions;

namespace RateTheWork.Infrastructure.Jobs;

/// <summary>
/// Blockchain senkronizasyon arka plan işi
/// </summary>
public class BlockchainSyncJob
{
    private readonly IBlockchainApplicationService _blockchainService;
    private readonly IReviewRepository _reviewRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<BlockchainSyncJob> _logger;

    public BlockchainSyncJob(
        IBlockchainApplicationService blockchainService,
        IReviewRepository reviewRepository,
        IUserRepository userRepository,
        ILogger<BlockchainSyncJob> logger)
    {
        _blockchainService = blockchainService;
        _reviewRepository = reviewRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Blockchain'e kaydedilmemiş yorumları senkronize eder
    /// </summary>
    public async Task SyncPendingReviewsAsync()
    {
        try
        {
            _logger.LogInformation("Starting blockchain sync for pending reviews");

            // Blockchain'e kaydedilmemiş yorumları getir
            var pendingReviews = await _reviewRepository.GetAllAsync();
            var filteredReviews = pendingReviews
                .Where(r => r.IsActive && r.IsPublished && !r.IsStoredOnBlockchain)
                .Take(100)
                .ToList();
            
            if (!filteredReviews.Any())
            {
                _logger.LogInformation("No pending reviews to sync");
                return;
            }

            var reviewIds = filteredReviews.Select(r => r.Id).ToList();
            var result = await _blockchainService.BulkStoreReviewsOnBlockchainAsync(reviewIds);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully synced {Count} reviews to blockchain", result.Data);
            }
            else
            {
                _logger.LogWarning("Failed to sync reviews to blockchain: {Error}", result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during blockchain sync job");
        }
    }

    /// <summary>
    /// Blockchain kimliği olmayan kullanıcılar için kimlik oluşturur
    /// </summary>
    public async Task CreateMissingBlockchainIdentitiesAsync()
    {
        try
        {
            _logger.LogInformation("Starting blockchain identity creation for users");

            // Blockchain kimliği olmayan aktif kullanıcıları getir
            var allUsers = await _userRepository.GetAllAsync();
            var usersWithoutBlockchain = allUsers
                .Where(u => u.IsActive && !u.IsBlockchainVerified)
                .ToList();

            foreach (var user in usersWithoutBlockchain.Take(10)) // Batch olarak işle
            {
                try
                {
                    var result = await _blockchainService.CreateUserBlockchainIdentityAsync(
                        user.Id.ToString());

                    if (result.IsSuccess)
                    {
                        _logger.LogInformation("Created blockchain identity for user {UserId}", user.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to create blockchain identity for user {UserId}: {Error}", 
                            user.Id, result.Error);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating blockchain identity for user {UserId}", user.Id);
                }

                // Rate limiting için kısa bekleme
                await Task.Delay(1000);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during blockchain identity creation job");
        }
    }

    /// <summary>
    /// Blockchain'deki verilerin bütünlüğünü kontrol eder
    /// </summary>
    public async Task VerifyBlockchainIntegrityAsync()
    {
        try
        {
            _logger.LogInformation("Starting blockchain integrity verification");

            // Son 24 saatte blockchain'e kaydedilmiş yorumları getir
            var allReviews = await _reviewRepository.GetAllAsync();
            var recentBlockchainReviews = allReviews
                .Where(r => r.IsStoredOnBlockchain && 
                       r.BlockchainStoredAt.HasValue && 
                       r.BlockchainStoredAt.Value > DateTime.UtcNow.AddDays(-1))
                .Take(100)
                .ToList();

            var integrityIssues = 0;
            foreach (var review in recentBlockchainReviews)
            {
                try
                {
                    var result = await _blockchainService.VerifyReviewIntegrityAsync(review.Id);
                    
                    if (result.IsSuccess && result.Data != null && !result.Data.IsIntegrityValid)
                    {
                        integrityIssues++;
                        _logger.LogWarning("Integrity issue detected for review {ReviewId}", review.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error verifying integrity for review {ReviewId}", review.Id);
                }
            }

            if (integrityIssues > 0)
            {
                _logger.LogWarning("Found {Count} integrity issues in blockchain data", integrityIssues);
            }
            else
            {
                _logger.LogInformation("All blockchain data integrity checks passed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during blockchain integrity verification job");
        }
    }

    /// <summary>
    /// Blockchain istatistiklerini günceller ve raporlar
    /// </summary>
    public async Task UpdateBlockchainStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Updating blockchain statistics");

            var result = await _blockchainService.GetBlockchainStatisticsAsync();
            
            if (result.IsSuccess)
            {
                var stats = result.Data;
                _logger.LogInformation(
                    "Blockchain Statistics - Users: {Users}, Reviews: {Reviews}, Companies: {Companies}, Total Transactions: {Transactions}",
                    stats.TotalUsersWithBlockchainIdentity,
                    stats.TotalReviewsOnBlockchain,
                    stats.TotalCompaniesVerified,
                    stats.TotalTransactions);
            }
            else
            {
                _logger.LogWarning("Failed to get blockchain statistics: {Error}", result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating blockchain statistics");
        }
    }
}