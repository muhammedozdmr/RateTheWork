using System.Security.Cryptography;
using System.Text;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.Blockchain;
using RateTheWork.Domain.ValueObjects.Blockchain;

namespace RateTheWork.Domain.Services;

public class BlockchainDomainService
{
    private readonly IBlockchainService _blockchainService;
    private readonly ISmartContractService _smartContractService;
    
    public BlockchainDomainService(
        IBlockchainService blockchainService,
        ISmartContractService smartContractService)
    {
        _blockchainService = blockchainService;
        _smartContractService = smartContractService;
    }
    
    /// <summary>
    /// Kullanıcı için blockchain kimliği oluşturur
    /// </summary>
    public async Task<UserBlockchainIdentity> CreateUserBlockchainIdentityAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        if (user.HasBlockchainIdentity())
            throw new BusinessRuleException("Kullanıcının zaten bir blockchain kimliği var.");
            
        // Yeni blockchain kimliği oluştur
        var identity = await _blockchainService.CreateUserIdentityAsync(Guid.Parse(user.Id), cancellationToken);
        
        // Smart contract deploy et
        var contractAddress = await _smartContractService.DeployUserIdentityContractAsync(
            identity.WalletAddress,
            cancellationToken);
            
        // Contract adresini identity'e ekle
        identity = identity.SetIdentityContract(contractAddress);
        
        return identity;
    }
    
    /// <summary>
    /// Yorumu blockchain'e kaydet
    /// </summary>
    public async Task<BlockchainTransaction> StoreReviewOnBlockchainAsync(
        Review review,
        User user,
        CancellationToken cancellationToken = default)
    {
        if (!user.HasBlockchainIdentity())
            throw new BusinessRuleException("Kullanıcının blockchain kimliği bulunmuyor.");
            
        if (review.IsStoredOnBlockchain)
            throw new BusinessRuleException("Yorum zaten blockchain'de saklanıyor.");
            
        // Review verilerini hash'le
        var reviewHash = GenerateReviewHash(review);
        
        // Kullanıcının blockchain kimliğini oluştur
        var walletAddress = BlockchainAddress.Create(user.BlockchainWalletAddress!);
        var identity = UserBlockchainIdentity.Create(
            walletAddress,
            user.EncryptedBlockchainPrivateKey!,
            user.BlockchainPublicKey!);
        
        if (!string.IsNullOrWhiteSpace(user.BlockchainIdentityContractAddress))
            identity = identity.SetIdentityContract(user.BlockchainIdentityContractAddress);
        
        // Blockchain'e kaydet
        var transaction = await _blockchainService.CreateAnonymousReviewAsync(
            identity,
            Guid.Parse(review.CompanyId),
            reviewHash,
            (int)Math.Round(review.OverallRating),
            cancellationToken);
            
        return transaction;
    }
    
    /// <summary>
    /// Kullanıcının tüm yorumlarını blockchain'den getir
    /// </summary>
    public async Task<List<string>> GetUserReviewsFromBlockchainAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        if (!user.HasBlockchainIdentity())
            throw new BusinessRuleException("Kullanıcının blockchain kimliği bulunmuyor.");
            
        var walletAddress = BlockchainAddress.Create(user.BlockchainWalletAddress!);
        
        return await _blockchainService.GetUserReviewHashesAsync(walletAddress, cancellationToken);
    }
    
    /// <summary>
    /// Yorum sahipliğini doğrula
    /// </summary>
    public async Task<bool> VerifyReviewOwnershipAsync(
        Review review,
        User user,
        CancellationToken cancellationToken = default)
    {
        if (!user.HasBlockchainIdentity())
            return false;
            
        if (!review.IsStoredOnBlockchain)
            return false;
            
        var walletAddress = BlockchainAddress.Create(user.BlockchainWalletAddress!);
        
        return await _blockchainService.VerifyReviewOwnershipAsync(
            walletAddress,
            review.BlockchainDataHash!,
            cancellationToken);
    }
    
    /// <summary>
    /// Kullanıcı kimliğini doğrula
    /// </summary>
    public async Task<bool> VerifyUserIdentityAsync(
        User user,
        string signature,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (!user.HasBlockchainIdentity())
            throw new BusinessRuleException("Kullanıcının blockchain kimliği bulunmuyor.");
            
        var walletAddress = BlockchainAddress.Create(user.BlockchainWalletAddress!);
        
        return await _blockchainService.VerifyUserIdentityAsync(
            walletAddress,
            signature,
            message,
            cancellationToken);
    }
    
    /// <summary>
    /// Review hash'i oluştur
    /// </summary>
    private static string GenerateReviewHash(Review review)
    {
        var dataToHash = $"{review.Id}|{review.UserId}|{review.CompanyId}|" +
                        $"{review.CommentType}|{review.OverallRating}|" +
                        $"{review.CommentText}|{review.CreatedAt:O}";
                        
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(dataToHash);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
    
    /// <summary>
    /// Kullanıcı verisini blockchain'e kaydet
    /// </summary>
    public async Task<BlockchainTransaction> StoreUserDataOnBlockchainAsync(
        User user,
        string dataType,
        string dataHash,
        CancellationToken cancellationToken = default)
    {
        if (!user.HasBlockchainIdentity())
            throw new BusinessRuleException("Kullanıcının blockchain kimliği bulunmuyor.");
            
        var walletAddress = BlockchainAddress.Create(user.BlockchainWalletAddress!);
        var identity = UserBlockchainIdentity.Create(
            walletAddress,
            user.EncryptedBlockchainPrivateKey!,
            user.BlockchainPublicKey!);
            
        return await _blockchainService.StoreUserDataHashAsync(
            identity,
            dataHash,
            dataType,
            cancellationToken);
    }
    
    /// <summary>
    /// Review bütünlüğünü kontrol et
    /// </summary>
    public bool VerifyReviewIntegrity(Review review)
    {
        if (!review.IsStoredOnBlockchain)
            return true; // Blockchain'de değilse, kontrol edilemez ama geçerli sayılır
            
        var currentHash = GenerateReviewHash(review);
        return review.VerifyBlockchainIntegrity(currentHash);
    }
}