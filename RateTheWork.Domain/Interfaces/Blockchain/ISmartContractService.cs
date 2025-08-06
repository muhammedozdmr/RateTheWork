using RateTheWork.Domain.ValueObjects.Blockchain;

namespace RateTheWork.Domain.Interfaces.Blockchain;

public interface ISmartContractService
{
    Task<string> DeployUserIdentityContractAsync(
        BlockchainAddress ownerAddress,
        CancellationToken cancellationToken = default);
    
    Task<string> DeployReviewContractAsync(
        BlockchainAddress ownerAddress,
        CancellationToken cancellationToken = default);
    
    Task<BlockchainTransaction> CallContractMethodAsync(
        string contractAddress,
        string methodName,
        object[] parameters,
        UserBlockchainIdentity identity,
        CancellationToken cancellationToken = default);
    
    Task<T> ReadContractDataAsync<T>(
        string contractAddress,
        string methodName,
        object[] parameters,
        CancellationToken cancellationToken = default);
    
    Task<bool> VerifyContractDeploymentAsync(
        string contractAddress,
        CancellationToken cancellationToken = default);
    
    Task<string> GetContractAbiAsync(
        string contractType,
        CancellationToken cancellationToken = default);
}