using RateTheWork.Application.Common.Models;
using RateTheWork.Application.Features.Blockchain.Commands.CreateUserBlockchainIdentity;
using RateTheWork.Application.Features.Blockchain.Commands.StoreReviewOnBlockchain;
using RateTheWork.Application.Features.Blockchain.Queries.VerifyReviewIntegrity;

namespace RateTheWork.Application.Services.Interfaces;

public interface IBlockchainApplicationService
{
    /// <summary>
    /// Kullanıcı için blockchain kimliği oluşturur
    /// </summary>
    Task<Result<UserBlockchainIdentityDto>> CreateUserBlockchainIdentityAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Yorumu blockchain'e kaydet
    /// </summary>
    Task<Result<BlockchainTransactionDto>> StoreReviewOnBlockchainAsync(
        string reviewId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının blockchain kimliğini getir
    /// </summary>
    Task<Result<UserBlockchainIdentityDto>> GetUserBlockchainIdentityAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Yorum bütünlüğünü kontrol et
    /// </summary>
    Task<Result<ReviewIntegrityDto>> VerifyReviewIntegrityAsync(
        string reviewId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının tüm blockchain yorumlarını getir
    /// </summary>
    Task<Result<List<string>>> GetUserBlockchainReviewsAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Şirket için blockchain doğrulaması yap
    /// </summary>
    Task<Result<bool>> VerifyCompanyOnBlockchainAsync(
        string companyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Toplu yorum blockchain'e kaydetme
    /// </summary>
    Task<Result<int>> BulkStoreReviewsOnBlockchainAsync(
        List<string> reviewIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Blockchain istatistiklerini getir
    /// </summary>
    Task<Result<BlockchainStatisticsDto>> GetBlockchainStatisticsAsync(
        CancellationToken cancellationToken = default);
}

public sealed class BlockchainStatisticsDto
{
    public int TotalUsersWithBlockchainIdentity { get; set; }
    public int TotalReviewsOnBlockchain { get; set; }
    public int TotalCompaniesVerified { get; set; }
    public int TotalTransactions { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public Dictionary<string, int> TransactionsByType { get; set; } = new();
}