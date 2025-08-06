namespace RateTheWork.Domain.Events.Review;

public sealed class ReviewStoredOnBlockchainEvent : DomainEventBase
{
    public ReviewStoredOnBlockchainEvent(
        string reviewId,
        string userId,
        string companyId,
        string transactionHash,
        string dataHash,
        string contractAddress,
        DateTime storedAt)
    {
        ReviewId = reviewId;
        UserId = userId;
        CompanyId = companyId;
        TransactionHash = transactionHash;
        DataHash = dataHash;
        ContractAddress = contractAddress;
        StoredAt = storedAt;
    }
    
    public string ReviewId { get; }
    public string UserId { get; }
    public string CompanyId { get; }
    public string TransactionHash { get; }
    public string DataHash { get; }
    public string ContractAddress { get; }
    public DateTime StoredAt { get; }
}

public sealed class ReviewBlockchainHashUpdatedEvent : DomainEventBase
{
    public ReviewBlockchainHashUpdatedEvent(
        string reviewId,
        string oldHash,
        string newHash,
        DateTime updatedAt)
    {
        ReviewId = reviewId;
        OldHash = oldHash;
        NewHash = newHash;
        UpdatedAt = updatedAt;
    }
    
    public string ReviewId { get; }
    public string OldHash { get; }
    public string NewHash { get; }
    public DateTime UpdatedAt { get; }
}

public sealed class ReviewBlockchainVerificationFailedEvent : DomainEventBase
{
    public ReviewBlockchainVerificationFailedEvent(
        string reviewId,
        string expectedHash,
        string actualHash,
        DateTime failedAt)
    {
        ReviewId = reviewId;
        ExpectedHash = expectedHash;
        ActualHash = actualHash;
        FailedAt = failedAt;
    }
    
    public string ReviewId { get; }
    public string ExpectedHash { get; }
    public string ActualHash { get; }
    public DateTime FailedAt { get; }
}