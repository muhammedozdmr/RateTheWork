using MediatR;
using RateTheWork.Application.Common.Models;

namespace RateTheWork.Application.Features.Blockchain.Commands.CreateUserBlockchainIdentity;

public sealed record CreateUserBlockchainIdentityCommand(
    string UserId
) : IRequest<Result<UserBlockchainIdentityDto>>;

public sealed class UserBlockchainIdentityDto
{
    public string WalletAddress { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string? ContractAddress { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}