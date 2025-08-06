namespace RateTheWork.Domain.Events.Company;

public sealed class CompanyVerifiedOnBlockchainEvent : DomainEventBase
{
    public CompanyVerifiedOnBlockchainEvent(
        string companyId,
        string walletAddress,
        string contractAddress,
        string dataHash,
        DateTime verifiedAt)
    {
        CompanyId = companyId;
        WalletAddress = walletAddress;
        ContractAddress = contractAddress;
        DataHash = dataHash;
        VerifiedAt = verifiedAt;
    }
    
    public string CompanyId { get; }
    public string WalletAddress { get; }
    public string ContractAddress { get; }
    public string DataHash { get; }
    public DateTime VerifiedAt { get; }
}

public sealed class CompanyBlockchainDataUpdatedEvent : DomainEventBase
{
    public CompanyBlockchainDataUpdatedEvent(
        string companyId,
        string oldHash,
        string newHash,
        DateTime updatedAt)
    {
        CompanyId = companyId;
        OldHash = oldHash;
        NewHash = newHash;
        UpdatedAt = updatedAt;
    }
    
    public string CompanyId { get; }
    public string OldHash { get; }
    public string NewHash { get; }
    public DateTime UpdatedAt { get; }
}

public sealed class CompanyBlockchainTransactionAddedEvent : DomainEventBase
{
    public CompanyBlockchainTransactionAddedEvent(
        string companyId,
        string transactionHash,
        string transactionType,
        DateTime addedAt)
    {
        CompanyId = companyId;
        TransactionHash = transactionHash;
        TransactionType = transactionType;
        AddedAt = addedAt;
    }
    
    public string CompanyId { get; }
    public string TransactionHash { get; }
    public string TransactionType { get; }
    public DateTime AddedAt { get; }
}