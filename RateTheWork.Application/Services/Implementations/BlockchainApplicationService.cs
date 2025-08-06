using MediatR;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Application.Common.Models;
using RateTheWork.Application.Features.Blockchain.Commands.CreateUserBlockchainIdentity;
using RateTheWork.Application.Features.Blockchain.Commands.StoreReviewOnBlockchain;
using RateTheWork.Application.Features.Blockchain.Queries.GetUserBlockchainIdentity;
using RateTheWork.Application.Features.Blockchain.Queries.VerifyReviewIntegrity;
using RateTheWork.Application.Services.Interfaces;
using RateTheWork.Domain.Interfaces.Blockchain;
using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.Services;
using RateTheWork.Domain.ValueObjects.Blockchain;

namespace RateTheWork.Application.Services.Implementations;

public class BlockchainApplicationService : IBlockchainApplicationService
{
    private readonly IMediator _mediator;
    private readonly IUserRepository _userRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IBlockchainService _blockchainService;
    private readonly ISmartContractService _smartContractService;
    private readonly ICacheService _cacheService;
    private readonly IMetricsService _metricsService;
    private readonly ILogger<BlockchainApplicationService> _logger;

    public BlockchainApplicationService(
        IMediator mediator,
        IUserRepository userRepository,
        IReviewRepository reviewRepository,
        ICompanyRepository companyRepository,
        IBlockchainService blockchainService,
        ISmartContractService smartContractService,
        ICacheService cacheService,
        IMetricsService metricsService,
        ILogger<BlockchainApplicationService> logger)
    {
        _mediator = mediator;
        _userRepository = userRepository;
        _reviewRepository = reviewRepository;
        _companyRepository = companyRepository;
        _blockchainService = blockchainService;
        _smartContractService = smartContractService;
        _cacheService = cacheService;
        _metricsService = metricsService;
        _logger = logger;
    }

    public async Task<Result<UserBlockchainIdentityDto>> CreateUserBlockchainIdentityAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateUserBlockchainIdentityCommand(userId);
        return await _mediator.Send(command, cancellationToken);
    }

    public async Task<Result<BlockchainTransactionDto>> StoreReviewOnBlockchainAsync(
        string reviewId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var command = new StoreReviewOnBlockchainCommand(reviewId, userId);
        return await _mediator.Send(command, cancellationToken);
    }

    public async Task<Result<UserBlockchainIdentityDto>> GetUserBlockchainIdentityAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserBlockchainIdentityQuery(userId);
        return await _mediator.Send(query, cancellationToken);
    }

    public async Task<Result<ReviewIntegrityDto>> VerifyReviewIntegrityAsync(
        string reviewId,
        CancellationToken cancellationToken = default)
    {
        var query = new VerifyReviewIntegrityQuery(reviewId);
        return await _mediator.Send(query, cancellationToken);
    }

    public async Task<Result<List<string>>> GetUserBlockchainReviewsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Result<List<string>>.Failure("Kullanıcı bulunamadı.");
            }

            if (!user.HasBlockchainIdentity())
            {
                return Result<List<string>>.Failure("Kullanıcının blockchain kimliği bulunmuyor.");
            }

            var walletAddress = BlockchainAddress.Create(user.BlockchainWalletAddress!);
            var reviewHashes = await _blockchainService.GetUserReviewHashesAsync(walletAddress, cancellationToken);

            return Result<List<string>>.Success(reviewHashes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blockchain reviews for user {UserId}", userId);
            return Result<List<string>>.Failure("Blockchain yorumları alınırken bir hata oluştu.");
        }
    }

    public async Task<Result<bool>> VerifyCompanyOnBlockchainAsync(
        string companyId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null)
            {
                return Result<bool>.Failure("Şirket bulunamadı.");
            }

            if (!company.IsVerifiedOnBlockchain)
            {
                return Result<bool>.Success(false);
            }

            // Blockchain'den doğrulama kontrolü yap
            var isValid = await _smartContractService.VerifyContractDeploymentAsync(
                company.BlockchainContractAddress!,
                cancellationToken);

            return Result<bool>.Success(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying company {CompanyId} on blockchain", companyId);
            return Result<bool>.Failure("Şirket doğrulaması yapılırken bir hata oluştu.");
        }
    }

    public async Task<Result<int>> BulkStoreReviewsOnBlockchainAsync(
        List<string> reviewIds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var successCount = 0;
            var startTime = DateTime.UtcNow;

            foreach (var reviewId in reviewIds)
            {
                try
                {
                    var review = await _reviewRepository.GetByIdAsync(reviewId);
                    if (review == null || review.IsStoredOnBlockchain)
                    {
                        continue;
                    }

                    var result = await StoreReviewOnBlockchainAsync(reviewId, review.UserId, cancellationToken);
                    if (result.IsSuccess)
                    {
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to store review {ReviewId} on blockchain", reviewId);
                }
            }

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            await _metricsService.RecordBlockchainOperationAsync("bulk_store_reviews", true, duration);

            return Result<int>.Success(successCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk storing reviews on blockchain");
            return Result<int>.Failure("Toplu kayıt işlemi sırasında bir hata oluştu.");
        }
    }

    public async Task<Result<BlockchainStatisticsDto>> GetBlockchainStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = "blockchain:statistics";
            var cached = await _cacheService.GetAsync<BlockchainStatisticsDto>(cacheKey, cancellationToken);
            if (cached != null)
            {
                return Result<BlockchainStatisticsDto>.Success(cached);
            }

            // İstatistikleri topla
            var usersWithBlockchain = await _userRepository.CountAsync(
                u => u.IsBlockchainVerified);

            var reviewsOnBlockchain = await _reviewRepository.CountAsync(
                r => r.IsStoredOnBlockchain);

            var verifiedCompanies = await _companyRepository.CountAsync(
                c => c.IsVerifiedOnBlockchain);

            var statistics = new BlockchainStatisticsDto
            {
                TotalUsersWithBlockchainIdentity = usersWithBlockchain,
                TotalReviewsOnBlockchain = reviewsOnBlockchain,
                TotalCompaniesVerified = verifiedCompanies,
                TotalTransactions = usersWithBlockchain + reviewsOnBlockchain + verifiedCompanies,
                LastTransactionDate = DateTime.UtcNow,
                TransactionsByType = new Dictionary<string, int>
                {
                    ["identity_created"] = usersWithBlockchain,
                    ["review_stored"] = reviewsOnBlockchain,
                    ["company_verified"] = verifiedCompanies
                }
            };

            // Cache'e kaydet (5 dakika)
            await _cacheService.SetAsync(cacheKey, statistics, TimeSpan.FromMinutes(5), cancellationToken);

            return Result<BlockchainStatisticsDto>.Success(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blockchain statistics");
            return Result<BlockchainStatisticsDto>.Failure("Blockchain istatistikleri alınırken bir hata oluştu.");
        }
    }
}