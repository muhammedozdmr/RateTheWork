namespace RateTheWork.Domain.Events.User;

public sealed class UserBlockchainIdentityCreatedEvent : DomainEventBase
{
    public UserBlockchainIdentityCreatedEvent(
        string userId,
        string walletAddress,
        string publicKey,
        DateTime createdAt)
    {
        UserId = userId;
        WalletAddress = walletAddress;
        PublicKey = publicKey;
        CreatedAt = createdAt;
    }
    
    public string UserId { get; }
    public string WalletAddress { get; }
    public string PublicKey { get; }
    public DateTime CreatedAt { get; }
}

public sealed class UserBlockchainContractDeployedEvent : DomainEventBase
{
    public UserBlockchainContractDeployedEvent(
        string userId,
        string contractAddress,
        DateTime deployedAt)
    {
        UserId = userId;
        ContractAddress = contractAddress;
        DeployedAt = deployedAt;
    }
    
    public string UserId { get; }
    public string ContractAddress { get; }
    public DateTime DeployedAt { get; }
}

public sealed class UserBlockchainIdentityVerifiedEvent : DomainEventBase
{
    public UserBlockchainIdentityVerifiedEvent(
        string userId,
        string walletAddress,
        DateTime verifiedAt)
    {
        UserId = userId;
        WalletAddress = walletAddress;
        VerifiedAt = verifiedAt;
    }
    
    public string UserId { get; }
    public string WalletAddress { get; }
    public DateTime VerifiedAt { get; }
}

public sealed class UserBlockchainTransactionExecutedEvent : DomainEventBase
{
    public UserBlockchainTransactionExecutedEvent(
        string userId,
        string transactionHash,
        string transactionType,
        DateTime executedAt)
    {
        UserId = userId;
        TransactionHash = transactionHash;
        TransactionType = transactionType;
        ExecutedAt = executedAt;
    }
    
    public string UserId { get; }
    public string TransactionHash { get; }
    public string TransactionType { get; }
    public DateTime ExecutedAt { get; }
}