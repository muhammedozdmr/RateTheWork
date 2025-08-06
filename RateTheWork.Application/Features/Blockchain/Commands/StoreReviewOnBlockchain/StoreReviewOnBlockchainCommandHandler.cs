using MediatR;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Application.Common.Models;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.Blockchain;
using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.Services;

namespace RateTheWork.Application.Features.Blockchain.Commands.StoreReviewOnBlockchain;

public sealed class StoreReviewOnBlockchainCommandHandler : IRequestHandler<StoreReviewOnBlockchainCommand, Result<BlockchainTransactionDto>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBlockchainService _blockchainService;
    private readonly ISmartContractService _smartContractService;
    private readonly Application.Common.Interfaces.IUnitOfWork _unitOfWork;
    private readonly ILogger<StoreReviewOnBlockchainCommandHandler> _logger;

    public StoreReviewOnBlockchainCommandHandler(
        IReviewRepository reviewRepository,
        IUserRepository userRepository,
        IBlockchainService blockchainService,
        ISmartContractService smartContractService,
        Application.Common.Interfaces.IUnitOfWork unitOfWork,
        ILogger<StoreReviewOnBlockchainCommandHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _userRepository = userRepository;
        _blockchainService = blockchainService;
        _smartContractService = smartContractService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<BlockchainTransactionDto>> Handle(
        StoreReviewOnBlockchainCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Review'i getir
            var review = await _reviewRepository.GetByIdAsync(request.ReviewId);
            if (review == null)
            {
                return Result<BlockchainTransactionDto>.Failure("Yorum bulunamadı.");
            }

            // Kullanıcıyı getir
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return Result<BlockchainTransactionDto>.Failure("Kullanıcı bulunamadı.");
            }

            // Kullanıcının review'in sahibi olduğunu kontrol et
            if (review.UserId != user.Id)
            {
                return Result<BlockchainTransactionDto>.Failure("Bu yorumu blockchain'e kaydetme yetkiniz yok.");
            }

            // BlockchainDomainService oluştur
            var blockchainDomainService = new BlockchainDomainService(_blockchainService, _smartContractService);

            // Review'i blockchain'e kaydet
            var transaction = await blockchainDomainService.StoreReviewOnBlockchainAsync(review, user, cancellationToken);

            // Review entity'sini güncelle
            review.StoreOnBlockchain(
                transaction.TransactionHash,
                transaction.Data ?? string.Empty,
                transaction.ToAddress.Value);

            // Değişiklikleri kaydet
            await _reviewRepository.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // DTO'ya map et
            var dto = new BlockchainTransactionDto
            {
                TransactionHash = transaction.TransactionHash,
                FromAddress = transaction.FromAddress.Value,
                ToAddress = transaction.ToAddress.Value,
                DataHash = transaction.Data ?? string.Empty,
                ContractAddress = transaction.ToAddress.Value,
                Timestamp = transaction.Timestamp,
                Status = transaction.Status.ToString()
            };

            _logger.LogInformation("Review {ReviewId} stored on blockchain with transaction {TransactionHash}",
                request.ReviewId, dto.TransactionHash);

            return Result<BlockchainTransactionDto>.Success(dto);
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while storing review {ReviewId} on blockchain",
                request.ReviewId);
            return Result<BlockchainTransactionDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing review {ReviewId} on blockchain", request.ReviewId);
            return Result<BlockchainTransactionDto>.Failure("Yorum blockchain'e kaydedilirken bir hata oluştu.");
        }
    }
}