using MediatR;
using RateTheWork.Application.Common.Models;

namespace RateTheWork.Application.Features.Blockchain.Queries.VerifyReviewIntegrity;

public sealed record VerifyReviewIntegrityQuery(
    string ReviewId
) : IRequest<Result<ReviewIntegrityDto>>;

public sealed class ReviewIntegrityDto
{
    public string ReviewId { get; set; } = string.Empty;
    public bool IsStoredOnBlockchain { get; set; }
    public bool IsIntegrityValid { get; set; }
    public string? BlockchainTransactionHash { get; set; }
    public string? BlockchainDataHash { get; set; }
    public string? CurrentDataHash { get; set; }
    public DateTime? BlockchainStoredAt { get; set; }
}