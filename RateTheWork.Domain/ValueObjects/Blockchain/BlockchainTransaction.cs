using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.ValueObjects.Common;

namespace RateTheWork.Domain.ValueObjects.Blockchain;

public sealed class BlockchainTransaction : ValueObject
{
    public string TransactionHash { get; }
    public BlockchainAddress FromAddress { get; }
    public BlockchainAddress ToAddress { get; }
    public DateTime Timestamp { get; }
    public long BlockNumber { get; }
    public TransactionStatus Status { get; }
    public string? Data { get; }
    
    private BlockchainTransaction(
        string transactionHash,
        BlockchainAddress fromAddress,
        BlockchainAddress toAddress,
        DateTime timestamp,
        long blockNumber,
        TransactionStatus status,
        string? data = null)
    {
        TransactionHash = transactionHash;
        FromAddress = fromAddress;
        ToAddress = toAddress;
        Timestamp = timestamp;
        BlockNumber = blockNumber;
        Status = status;
        Data = data;
    }
    
    public static BlockchainTransaction Create(
        string transactionHash,
        BlockchainAddress fromAddress,
        BlockchainAddress toAddress,
        long blockNumber,
        string? data = null)
    {
        if (string.IsNullOrWhiteSpace(transactionHash))
            throw new DomainValidationException("transactionHash", "Transaction hash cannot be empty");
            
        if (blockNumber < 0)
            throw new DomainValidationException("blockNumber", "Block number cannot be negative");
            
        return new BlockchainTransaction(
            transactionHash,
            fromAddress,
            toAddress,
            DateTime.UtcNow,
            blockNumber,
            TransactionStatus.Pending,
            data);
    }
    
    public BlockchainTransaction MarkAsConfirmed()
    {
        return new BlockchainTransaction(
            TransactionHash,
            FromAddress,
            ToAddress,
            Timestamp,
            BlockNumber,
            TransactionStatus.Confirmed,
            Data);
    }
    
    public BlockchainTransaction MarkAsFailed()
    {
        return new BlockchainTransaction(
            TransactionHash,
            FromAddress,
            ToAddress,
            Timestamp,
            BlockNumber,
            TransactionStatus.Failed,
            Data);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TransactionHash;
        yield return FromAddress;
        yield return ToAddress;
        yield return Timestamp;
        yield return BlockNumber;
        yield return Status;
        if (Data != null) yield return Data;
    }
}

public enum TransactionStatus
{
    Pending,
    Confirmed,
    Failed
}