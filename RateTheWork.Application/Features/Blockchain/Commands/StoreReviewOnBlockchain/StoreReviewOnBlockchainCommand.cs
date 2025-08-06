using MediatR;
using RateTheWork.Application.Common.Models;

namespace RateTheWork.Application.Features.Blockchain.Commands.StoreReviewOnBlockchain;

public sealed record StoreReviewOnBlockchainCommand(
    string ReviewId,
    string UserId
) : IRequest<Result<BlockchainTransactionDto>>;

public sealed class BlockchainTransactionDto
{
    public string TransactionHash { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public string DataHash { get; set; } = string.Empty;
    public string ContractAddress { get; set; } = string.Empty;
    public long BlockNumber { get; set; }
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
}