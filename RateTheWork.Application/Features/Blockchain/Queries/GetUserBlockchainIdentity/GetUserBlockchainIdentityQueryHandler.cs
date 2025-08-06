using MediatR;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Models;
using RateTheWork.Application.Features.Blockchain.Commands.CreateUserBlockchainIdentity;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Application.Features.Blockchain.Queries.GetUserBlockchainIdentity;

public sealed class GetUserBlockchainIdentityQueryHandler : IRequestHandler<GetUserBlockchainIdentityQuery, Result<UserBlockchainIdentityDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserBlockchainIdentityQueryHandler> _logger;

    public GetUserBlockchainIdentityQueryHandler(
        IUserRepository userRepository,
        ILogger<GetUserBlockchainIdentityQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<UserBlockchainIdentityDto>> Handle(
        GetUserBlockchainIdentityQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Kullanıcıyı getir
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return Result<UserBlockchainIdentityDto>.Failure("Kullanıcı bulunamadı.");
            }

            // Blockchain kimliği yoksa hata dön
            if (!user.HasBlockchainIdentity())
            {
                return Result<UserBlockchainIdentityDto>.Failure("Kullanıcının blockchain kimliği bulunmuyor.");
            }

            // DTO'ya map et
            var dto = new UserBlockchainIdentityDto
            {
                WalletAddress = user.BlockchainWalletAddress!,
                PublicKey = user.BlockchainPublicKey!,
                ContractAddress = user.BlockchainIdentityContractAddress,
                IsActive = user.IsBlockchainVerified,
                CreatedAt = user.BlockchainVerifiedAt ?? DateTime.UtcNow
            };

            return Result<UserBlockchainIdentityDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blockchain identity for user {UserId}", request.UserId);
            return Result<UserBlockchainIdentityDto>.Failure("Blockchain kimliği alınırken bir hata oluştu.");
        }
    }
}