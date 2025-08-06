using MediatR;
using RateTheWork.Application.Common.Models;
using RateTheWork.Application.Features.Blockchain.Commands.CreateUserBlockchainIdentity;

namespace RateTheWork.Application.Features.Blockchain.Queries.GetUserBlockchainIdentity;

public sealed record GetUserBlockchainIdentityQuery(
    string UserId
) : IRequest<Result<UserBlockchainIdentityDto>>;