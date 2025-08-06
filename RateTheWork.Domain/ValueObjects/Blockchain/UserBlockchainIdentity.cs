using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.ValueObjects.Common;

namespace RateTheWork.Domain.ValueObjects.Blockchain;

public sealed class UserBlockchainIdentity : ValueObject
{
    public BlockchainAddress WalletAddress { get; }
    public string EncryptedPrivateKey { get; }
    public string PublicKey { get; }
    public DateTime CreatedAt { get; }
    public bool IsActive { get; }
    public string? IdentityContractAddress { get; }
    
    private UserBlockchainIdentity(
        BlockchainAddress walletAddress,
        string encryptedPrivateKey,
        string publicKey,
        DateTime createdAt,
        bool isActive,
        string? identityContractAddress = null)
    {
        WalletAddress = walletAddress;
        EncryptedPrivateKey = encryptedPrivateKey;
        PublicKey = publicKey;
        CreatedAt = createdAt;
        IsActive = isActive;
        IdentityContractAddress = identityContractAddress;
    }
    
    public static UserBlockchainIdentity Create(
        BlockchainAddress walletAddress,
        string encryptedPrivateKey,
        string publicKey)
    {
        if (string.IsNullOrWhiteSpace(encryptedPrivateKey))
            throw new DomainValidationException("encryptedPrivateKey", "Encrypted private key cannot be empty");
            
        if (string.IsNullOrWhiteSpace(publicKey))
            throw new DomainValidationException("publicKey", "Public key cannot be empty");
            
        return new UserBlockchainIdentity(
            walletAddress,
            encryptedPrivateKey,
            publicKey,
            DateTime.UtcNow,
            true);
    }
    
    public UserBlockchainIdentity SetIdentityContract(string contractAddress)
    {
        if (string.IsNullOrWhiteSpace(contractAddress))
            throw new DomainValidationException("contractAddress", "Contract address cannot be empty");
            
        return new UserBlockchainIdentity(
            WalletAddress,
            EncryptedPrivateKey,
            PublicKey,
            CreatedAt,
            IsActive,
            contractAddress);
    }
    
    public UserBlockchainIdentity Deactivate()
    {
        return new UserBlockchainIdentity(
            WalletAddress,
            EncryptedPrivateKey,
            PublicKey,
            CreatedAt,
            false,
            IdentityContractAddress);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return WalletAddress;
        yield return EncryptedPrivateKey;
        yield return PublicKey;
        yield return CreatedAt;
        yield return IsActive;
        if (IdentityContractAddress != null) yield return IdentityContractAddress;
    }
}