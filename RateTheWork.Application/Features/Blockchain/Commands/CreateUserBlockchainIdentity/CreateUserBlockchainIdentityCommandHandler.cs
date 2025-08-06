using MediatR;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Application.Common.Models;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.Blockchain;
using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.Services;

namespace RateTheWork.Application.Features.Blockchain.Commands.CreateUserBlockchainIdentity;

public sealed class CreateUserBlockchainIdentityCommandHandler : IRequestHandler<CreateUserBlockchainIdentityCommand, Result<UserBlockchainIdentityDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IBlockchainService _blockchainService;
    private readonly ISmartContractService _smartContractService;
    private readonly Application.Common.Interfaces.IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateUserBlockchainIdentityCommandHandler> _logger;

    public CreateUserBlockchainIdentityCommandHandler(
        IUserRepository userRepository,
        IBlockchainService blockchainService,
        ISmartContractService smartContractService,
        Application.Common.Interfaces.IUnitOfWork unitOfWork,
        ILogger<CreateUserBlockchainIdentityCommandHandler> logger)
    {
        _userRepository = userRepository;
        _blockchainService = blockchainService;
        _smartContractService = smartContractService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UserBlockchainIdentityDto>> Handle(
        CreateUserBlockchainIdentityCommand request,
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

            // Blockchain kimliği zaten varsa hata dön
            if (user.HasBlockchainIdentity())
            {
                return Result<UserBlockchainIdentityDto>.Failure("Kullanıcının zaten bir blockchain kimliği var.");
            }

            // BlockchainDomainService oluştur
            var blockchainDomainService = new BlockchainDomainService(_blockchainService, _smartContractService);

            // Blockchain kimliği oluştur
            var blockchainIdentity = await blockchainDomainService.CreateUserBlockchainIdentityAsync(user, cancellationToken);

            // User entity'sine blockchain kimliğini set et
            user.SetBlockchainIdentity(blockchainIdentity);

            // Değişiklikleri kaydet
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // DTO'ya map et
            var dto = new UserBlockchainIdentityDto
            {
                WalletAddress = blockchainIdentity.WalletAddress.Value,
                PublicKey = blockchainIdentity.PublicKey,
                ContractAddress = blockchainIdentity.IdentityContractAddress,
                IsActive = blockchainIdentity.IsActive,
                CreatedAt = blockchainIdentity.CreatedAt
            };

            _logger.LogInformation("Blockchain identity created for user {UserId} with wallet {WalletAddress}",
                request.UserId, dto.WalletAddress);

            return Result<UserBlockchainIdentityDto>.Success(dto);
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while creating blockchain identity for user {UserId}",
                request.UserId);
            return Result<UserBlockchainIdentityDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating blockchain identity for user {UserId}", request.UserId);
            return Result<UserBlockchainIdentityDto>.Failure("Blockchain kimliği oluşturulurken bir hata oluştu.");
        }
    }
}