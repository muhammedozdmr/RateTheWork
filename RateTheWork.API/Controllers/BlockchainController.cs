using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateTheWork.Application.Common.Models;
using RateTheWork.Application.Services.Interfaces;
using RateTheWork.Application.Features.Blockchain.Queries.VerifyReviewIntegrity;

namespace RateTheWork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BlockchainController : ControllerBase
{
    private readonly IBlockchainApplicationService _blockchainService;
    private readonly ILogger<BlockchainController> _logger;

    public BlockchainController(
        IBlockchainApplicationService blockchainService,
        ILogger<BlockchainController> logger)
    {
        _blockchainService = blockchainService;
        _logger = logger;
    }

    /// <summary>
    /// Kullanıcı için blockchain kimliği oluşturur
    /// </summary>
    [HttpPost("identity/create")]
    [ProducesResponseType(typeof(BlockchainIdentityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUserBlockchainIdentity(
        [FromBody] CreateBlockchainIdentityRequest request)
    {
        try
        {
            var result = await _blockchainService.CreateUserBlockchainIdentityAsync(
                request.UserId,
                HttpContext.RequestAborted);

            if (result.IsSuccess)
            {
                return Ok(new BlockchainIdentityResponse
                {
                    UserId = request.UserId,
                    WalletAddress = result.Data?.WalletAddress ?? string.Empty,
                    IsVerified = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return BadRequest(new ProblemDetails
            {
                Title = "Blockchain Identity Creation Failed",
                Detail = result.Error
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating blockchain identity for user {UserId}", request.UserId);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while creating blockchain identity"
            });
        }
    }

    /// <summary>
    /// Kullanıcının blockchain kimliğini getirir
    /// </summary>
    [HttpGet("identity/{userId}")]
    [ProducesResponseType(typeof(BlockchainIdentityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserBlockchainIdentity(string userId)
    {
        try
        {
            var result = await _blockchainService.GetUserBlockchainIdentityAsync(
                userId,
                HttpContext.RequestAborted);

            if (result.IsSuccess)
            {
                return Ok(new BlockchainIdentityResponse
                {
                    UserId = userId,
                    WalletAddress = result.Data?.WalletAddress ?? string.Empty,
                    IsVerified = result.Data?.IsVerified ?? false,
                    CreatedAt = result.Data?.CreatedAt ?? DateTime.UtcNow
                });
            }

            return NotFound(new ProblemDetails
            {
                Title = "Blockchain Identity Not Found",
                Detail = result.Error
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blockchain identity for user {UserId}", userId);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving blockchain identity"
            });
        }
    }

    /// <summary>
    /// Yorumu blockchain'e kaydeder
    /// </summary>
    [HttpPost("review/store")]
    [ProducesResponseType(typeof(BlockchainTransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StoreReviewOnBlockchain(
        [FromBody] StoreReviewRequest request)
    {
        try
        {
            var result = await _blockchainService.StoreReviewOnBlockchainAsync(
                request.ReviewId,
                request.UserId,
                HttpContext.RequestAborted);

            if (result.IsSuccess)
            {
                return Ok(new BlockchainTransactionResponse
                {
                    TransactionHash = result.Data?.TransactionHash ?? string.Empty,
                    BlockNumber = result.Data?.BlockNumber ?? 0,
                    Status = result.Data?.Status ?? "pending",
                    CreatedAt = DateTime.UtcNow
                });
            }

            return BadRequest(new ProblemDetails
            {
                Title = "Review Storage Failed",
                Detail = result.Error
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing review {ReviewId} on blockchain", request.ReviewId);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while storing review on blockchain"
            });
        }
    }

    /// <summary>
    /// Yorumun blockchain'deki bütünlüğünü doğrular
    /// </summary>
    [HttpGet("review/{reviewId}/verify")]
    [ProducesResponseType(typeof(ReviewIntegrityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyReviewIntegrity(string reviewId)
    {
        try
        {
            var result = await _blockchainService.VerifyReviewIntegrityAsync(
                reviewId,
                HttpContext.RequestAborted);

            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            return NotFound(new ProblemDetails
            {
                Title = "Review Not Found",
                Detail = result.Error
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying review {ReviewId} integrity", reviewId);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while verifying review integrity"
            });
        }
    }

    /// <summary>
    /// Kullanıcının blockchain'deki yorumlarını getirir
    /// </summary>
    [HttpGet("user/{userId}/reviews")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserBlockchainReviews(string userId)
    {
        try
        {
            var result = await _blockchainService.GetUserBlockchainReviewsAsync(
                userId,
                HttpContext.RequestAborted);

            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            return NotFound(new ProblemDetails
            {
                Title = "User Reviews Not Found",
                Detail = result.Error
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blockchain reviews for user {UserId}", userId);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving user reviews"
            });
        }
    }

    /// <summary>
    /// Şirketin blockchain doğrulamasını kontrol eder
    /// </summary>
    [HttpGet("company/{companyId}/verify")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyCompanyOnBlockchain(string companyId)
    {
        try
        {
            var result = await _blockchainService.VerifyCompanyOnBlockchainAsync(
                companyId,
                HttpContext.RequestAborted);

            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            return NotFound(new ProblemDetails
            {
                Title = "Company Not Found",
                Detail = result.Error
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying company {CompanyId} on blockchain", companyId);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while verifying company"
            });
        }
    }

    /// <summary>
    /// Yorumları toplu olarak blockchain'e kaydeder
    /// </summary>
    [HttpPost("review/bulk-store")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(BulkStoreResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkStoreReviewsOnBlockchain(
        [FromBody] BulkStoreReviewsRequest request)
    {
        try
        {
            var result = await _blockchainService.BulkStoreReviewsOnBlockchainAsync(
                request.ReviewIds,
                HttpContext.RequestAborted);

            if (result.IsSuccess)
            {
                return Ok(new BulkStoreResult
                {
                    SuccessCount = result.Data,
                    TotalCount = request.ReviewIds.Count,
                    Message = $"{result.Data} of {request.ReviewIds.Count} reviews successfully stored on blockchain"
                });
            }

            return BadRequest(new ProblemDetails
            {
                Title = "Bulk Storage Failed",
                Detail = result.Error
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk storing reviews on blockchain");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred during bulk storage operation"
            });
        }
    }

    /// <summary>
    /// Blockchain istatistiklerini getirir
    /// </summary>
    [HttpGet("statistics")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BlockchainStatisticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBlockchainStatistics()
    {
        try
        {
            var result = await _blockchainService.GetBlockchainStatisticsAsync(
                HttpContext.RequestAborted);

            if (result.IsSuccess)
            {
                return Ok(new BlockchainStatisticsResponse
                {
                    TotalUsersWithBlockchainIdentity = result.Data?.TotalUsersWithBlockchainIdentity ?? 0,
                    TotalReviewsOnBlockchain = result.Data?.TotalReviewsOnBlockchain ?? 0,
                    TotalCompaniesVerified = result.Data?.TotalCompaniesVerified ?? 0,
                    TotalTransactions = result.Data?.TotalTransactions ?? 0,
                    LastTransactionDate = result.Data?.LastTransactionDate ?? DateTime.UtcNow
                });
            }

            return StatusCode(500, new ProblemDetails
            {
                Title = "Statistics Retrieval Failed",
                Detail = result.Error
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blockchain statistics");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving statistics"
            });
        }
    }
}

// Request DTOs
public class CreateBlockchainIdentityRequest
{
    public string UserId { get; set; } = string.Empty;
}

public class StoreReviewRequest
{
    public string ReviewId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

public class BulkStoreReviewsRequest
{
    public List<string> ReviewIds { get; set; } = new();
}

// Response DTOs
public class BulkStoreResult
{
    public int SuccessCount { get; set; }
    public int TotalCount { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class BlockchainIdentityResponse
{
    public string UserId { get; set; } = string.Empty;
    public string WalletAddress { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BlockchainTransactionResponse
{
    public string TransactionHash { get; set; } = string.Empty;
    public long BlockNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class BlockchainStatisticsResponse
{
    public int TotalUsersWithBlockchainIdentity { get; set; }
    public int TotalReviewsOnBlockchain { get; set; }
    public int TotalCompaniesVerified { get; set; }
    public int TotalTransactions { get; set; }
    public DateTime LastTransactionDate { get; set; }
}