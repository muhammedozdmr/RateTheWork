using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Models;
using RateTheWork.Domain.Interfaces.Blockchain;
using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.Services;

namespace RateTheWork.Application.Features.Blockchain.Queries.VerifyReviewIntegrity;

public sealed class VerifyReviewIntegrityQueryHandler : IRequestHandler<VerifyReviewIntegrityQuery, Result<ReviewIntegrityDto>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IBlockchainService _blockchainService;
    private readonly ISmartContractService _smartContractService;
    private readonly ILogger<VerifyReviewIntegrityQueryHandler> _logger;

    public VerifyReviewIntegrityQueryHandler(
        IReviewRepository reviewRepository,
        IBlockchainService blockchainService,
        ISmartContractService smartContractService,
        ILogger<VerifyReviewIntegrityQueryHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _blockchainService = blockchainService;
        _smartContractService = smartContractService;
        _logger = logger;
    }

    public async Task<Result<ReviewIntegrityDto>> Handle(
        VerifyReviewIntegrityQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Review'i getir
            var review = await _reviewRepository.GetByIdAsync(request.ReviewId);
            if (review == null)
            {
                return Result<ReviewIntegrityDto>.Failure("Yorum bulunamadı.");
            }

            // BlockchainDomainService oluştur
            var blockchainDomainService = new BlockchainDomainService(_blockchainService, _smartContractService);

            // Review bütünlüğünü kontrol et
            var isIntegrityValid = blockchainDomainService.VerifyReviewIntegrity(review);

            // Mevcut hash'i hesapla
            var currentHash = GenerateReviewHash(review);

            // DTO'ya map et
            var dto = new ReviewIntegrityDto
            {
                ReviewId = review.Id,
                IsStoredOnBlockchain = review.IsStoredOnBlockchain,
                IsIntegrityValid = isIntegrityValid,
                BlockchainTransactionHash = review.BlockchainTransactionHash,
                BlockchainDataHash = review.BlockchainDataHash,
                CurrentDataHash = currentHash,
                BlockchainStoredAt = review.BlockchainStoredAt
            };

            if (!isIntegrityValid && review.IsStoredOnBlockchain)
            {
                _logger.LogWarning("Review {ReviewId} integrity check failed. Expected hash: {ExpectedHash}, Current hash: {CurrentHash}",
                    request.ReviewId, review.BlockchainDataHash, currentHash);
            }

            return Result<ReviewIntegrityDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying integrity for review {ReviewId}", request.ReviewId);
            return Result<ReviewIntegrityDto>.Failure("Yorum bütünlüğü kontrol edilirken bir hata oluştu.");
        }
    }

    private static string GenerateReviewHash(Domain.Entities.Review review)
    {
        var dataToHash = $"{review.Id}|{review.UserId}|{review.CompanyId}|" +
                        $"{review.CommentType}|{review.OverallRating}|" +
                        $"{review.CommentText}|{review.CreatedAt:O}";

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(dataToHash);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}