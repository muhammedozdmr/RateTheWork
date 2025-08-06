using RateTheWork.Domain.ValueObjects.Blockchain;

namespace RateTheWork.Domain.Interfaces.Blockchain;

public interface IBlockchainService
{
    Task<UserBlockchainIdentity> CreateUserIdentityAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<BlockchainTransaction> StoreUserDataHashAsync(
        UserBlockchainIdentity identity,
        string dataHash,
        string dataType,
        CancellationToken cancellationToken = default);
    
    Task<string> RetrieveUserDataHashAsync(
        BlockchainAddress walletAddress,
        string dataType,
        CancellationToken cancellationToken = default);
    
    Task<bool> VerifyUserIdentityAsync(
        BlockchainAddress walletAddress,
        string signature,
        string message,
        CancellationToken cancellationToken = default);
    
    Task<BlockchainTransaction> CreateAnonymousReviewAsync(
        UserBlockchainIdentity identity,
        Guid companyId,
        string reviewHash,
        int rating,
        CancellationToken cancellationToken = default);
    
    Task<List<string>> GetUserReviewHashesAsync(
        BlockchainAddress walletAddress,
        CancellationToken cancellationToken = default);
    
    Task<bool> VerifyReviewOwnershipAsync(
        BlockchainAddress walletAddress,
        string reviewHash,
        CancellationToken cancellationToken = default);
    
    Task<BlockchainAddress> GenerateNewWalletAddressAsync(CancellationToken cancellationToken = default);
    
    Task<string> EncryptPrivateKeyAsync(string privateKey, string passphrase);
    
    Task<string> DecryptPrivateKeyAsync(string encryptedPrivateKey, string passphrase);
}