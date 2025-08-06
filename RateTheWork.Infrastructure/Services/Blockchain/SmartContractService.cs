using System.Numerics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using RateTheWork.Domain.Interfaces.Blockchain;
using RateTheWork.Domain.ValueObjects.Blockchain;

namespace RateTheWork.Infrastructure.Services.Blockchain;

public class SmartContractService : ISmartContractService
{
    private readonly Web3 _web3;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmartContractService> _logger;
    private readonly Account _adminAccount;

    public SmartContractService(
        IConfiguration configuration,
        ILogger<SmartContractService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var rpcUrl = _configuration["Blockchain:RpcUrl"] ?? "http://localhost:8545";
        var privateKey = _configuration["Blockchain:AdminPrivateKey"] ?? GeneratePrivateKey();

        _adminAccount = new Account(privateKey);
        _web3 = new Web3(_adminAccount, rpcUrl);
    }

    public async Task<string> DeployUserIdentityContractAsync(
        BlockchainAddress ownerAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deploying user identity contract for owner {OwnerAddress}...", ownerAddress.Value);

            // Get contract bytecode and ABI from configuration or files
            var contractBytecode = await GetContractBytecodeAsync("UserIdentity");
            var contractAbi = await GetContractAbiAsync("UserIdentity", cancellationToken);

            var receipt = await _web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(
                contractBytecode,
                _adminAccount.Address,
                new HexBigInteger(3000000),
                null,
                null,
                ownerAddress.Value,
                cancellationToken);

            if (receipt.Status?.Value == 1)
            {
                _logger.LogInformation("User identity contract deployed successfully at address: {ContractAddress}", 
                    receipt.ContractAddress);
                return receipt.ContractAddress;
            }

            throw new InvalidOperationException("Contract deployment failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deploying user identity contract");
            throw;
        }
    }

    public async Task<string> DeployReviewContractAsync(
        BlockchainAddress ownerAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deploying review contract for owner {OwnerAddress}...", ownerAddress.Value);

            // Get contract bytecode and ABI from configuration or files
            var contractBytecode = await GetContractBytecodeAsync("Review");
            var contractAbi = await GetContractAbiAsync("Review", cancellationToken);

            var receipt = await _web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(
                contractBytecode,
                _adminAccount.Address,
                new HexBigInteger(3000000),
                null,
                null,
                ownerAddress.Value,
                cancellationToken);

            if (receipt.Status?.Value == 1)
            {
                _logger.LogInformation("Review contract deployed successfully at address: {ContractAddress}", 
                    receipt.ContractAddress);
                return receipt.ContractAddress;
            }

            throw new InvalidOperationException("Contract deployment failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deploying review contract");
            throw;
        }
    }

    public async Task<bool> VerifyContractDeploymentAsync(
        string contractAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var code = await _web3.Eth.GetCode.SendRequestAsync(contractAddress);
            
            // Eğer kod "0x" değilse ve boş değilse, contract deploy edilmiş demektir
            var isDeployed = !string.IsNullOrEmpty(code) && code != "0x" && code != "0x0";
            
            _logger.LogInformation("Contract {ContractAddress} deployment status: {IsDeployed}", 
                contractAddress, isDeployed);
            
            return isDeployed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying contract deployment at address {ContractAddress}", contractAddress);
            return false;
        }
    }

    public async Task<T> CallContractFunctionAsync<T>(
        string contractAddress,
        string contractAbi,
        string functionName,
        params object[] parameters)
    {
        try
        {
            var contract = _web3.Eth.GetContract(contractAbi, contractAddress);
            var function = contract.GetFunction(functionName);
            
            var result = await function.CallAsync<T>(parameters);
            
            _logger.LogDebug("Called contract function {FunctionName} on {ContractAddress}", 
                functionName, contractAddress);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling contract function {FunctionName} on {ContractAddress}", 
                functionName, contractAddress);
            throw;
        }
    }

    public async Task<string> SendContractTransactionAsync(
        string contractAddress,
        string contractAbi,
        string functionName,
        params object[] parameters)
    {
        try
        {
            var contract = _web3.Eth.GetContract(contractAbi, contractAddress);
            var function = contract.GetFunction(functionName);
            
            var receipt = await function.SendTransactionAndWaitForReceiptAsync(
                _adminAccount.Address,
                new HexBigInteger(900000),
                new HexBigInteger(0),
                null,
                parameters);

            if (receipt.Status?.Value == 1)
            {
                _logger.LogInformation("Contract transaction {FunctionName} successful: {TransactionHash}", 
                    functionName, receipt.TransactionHash);
                return receipt.TransactionHash;
            }

            throw new InvalidOperationException($"Contract transaction {functionName} failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending contract transaction {FunctionName} to {ContractAddress}", 
                functionName, contractAddress);
            throw;
        }
    }

    public async Task<BigInteger> GetContractBalanceAsync(
        string contractAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var balance = await _web3.Eth.GetBalance.SendRequestAsync(contractAddress);
            return balance.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contract balance for {ContractAddress}", contractAddress);
            throw;
        }
    }

    public async Task<List<EventLog<T>>> GetContractEventsAsync<T>(
        string contractAddress,
        string eventName,
        BlockParameter fromBlock,
        BlockParameter toBlock,
        CancellationToken cancellationToken = default) where T : IEventDTO, new()
    {
        try
        {
            var eventHandler = _web3.Eth.GetEvent<T>(contractAddress);
            
            var filter = eventHandler.CreateFilterInput(fromBlock, toBlock);
            var events = await eventHandler.GetAllChangesAsync(filter);
            
            _logger.LogInformation("Retrieved {EventCount} {EventName} events from contract {ContractAddress}", 
                events.Count, eventName, contractAddress);
            
            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting events {EventName} from contract {ContractAddress}", 
                eventName, contractAddress);
            throw;
        }
    }

    public async Task<bool> IsContractPausedAsync(
        string contractAddress,
        string contractAbi,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await CallContractFunctionAsync<bool>(
                contractAddress, 
                contractAbi, 
                "paused");
        }
        catch
        {
            // Contract might not have paused functionality
            return false;
        }
    }

    public async Task<string> PauseContractAsync(
        string contractAddress,
        string contractAbi,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await SendContractTransactionAsync(
                contractAddress,
                contractAbi,
                "pause");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing contract {ContractAddress}", contractAddress);
            throw;
        }
    }

    public async Task<string> UnpauseContractAsync(
        string contractAddress,
        string contractAbi,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await SendContractTransactionAsync(
                contractAddress,
                contractAbi,
                "unpause");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unpausing contract {ContractAddress}", contractAddress);
            throw;
        }
    }

    public async Task<string> TransferContractOwnershipAsync(
        string contractAddress,
        string contractAbi,
        string newOwnerAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await SendContractTransactionAsync(
                contractAddress,
                contractAbi,
                "transferOwnership",
                newOwnerAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring ownership of contract {ContractAddress} to {NewOwner}", 
                contractAddress, newOwnerAddress);
            throw;
        }
    }

    public async Task<string> GetContractOwnerAsync(
        string contractAddress,
        string contractAbi,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await CallContractFunctionAsync<string>(
                contractAddress,
                contractAbi,
                "owner");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting owner of contract {ContractAddress}", contractAddress);
            throw;
        }
    }

    public async Task<BlockchainTransaction> CallContractMethodAsync(
        string contractAddress,
        string methodName,
        object[] parameters,
        UserBlockchainIdentity identity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Decrypt user's private key
            var privateKey = await DecryptPrivateKeyAsync(
                identity.EncryptedPrivateKey,
                identity.WalletAddress.Value);

            var userAccount = new Account(privateKey);
            var userWeb3 = new Web3(userAccount, _web3.Client);

            var contractAbi = await GetContractAbiAsync("Generic", cancellationToken);
            var contract = userWeb3.Eth.GetContract(contractAbi, contractAddress);
            var function = contract.GetFunction(methodName);

            var receipt = await function.SendTransactionAndWaitForReceiptAsync(
                userAccount.Address,
                new HexBigInteger(900000),
                new HexBigInteger(0),
                null,
                parameters);

            var transaction = BlockchainTransaction.Create(
                receipt.TransactionHash,
                identity.WalletAddress,
                BlockchainAddress.Create(contractAddress),
                (long)receipt.BlockNumber.Value,
                methodName);

            if (receipt.Status?.Value == 1)
            {
                transaction = transaction.MarkAsConfirmed();
            }
            else
            {
                transaction = transaction.MarkAsFailed();
            }

            _logger.LogInformation("Contract method {MethodName} called successfully: {TransactionHash}",
                methodName, receipt.TransactionHash);

            return transaction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling contract method {MethodName} on {ContractAddress}",
                methodName, contractAddress);
            throw;
        }
    }

    public async Task<T> ReadContractDataAsync<T>(
        string contractAddress,
        string methodName,
        object[] parameters,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contractAbi = await GetContractAbiAsync("Generic", cancellationToken);
            return await CallContractFunctionAsync<T>(
                contractAddress,
                contractAbi,
                methodName,
                parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading contract data from {MethodName} on {ContractAddress}",
                methodName, contractAddress);
            throw;
        }
    }

    public async Task<string> GetContractAbiAsync(
        string contractType,
        CancellationToken cancellationToken = default)
    {
        // In production, these would be loaded from files or configuration
        return contractType switch
        {
            "UserIdentity" => GetUserIdentityContractAbi(),
            "Review" => GetReviewContractAbi(),
            "Generic" => GetGenericContractAbi(),
            _ => throw new ArgumentException($"Unknown contract type: {contractType}")
        };
    }

    private async Task<string> DecryptPrivateKeyAsync(string encryptedPrivateKey, string passphrase)
    {
        // Implementation would decrypt the private key
        // This is a simplified version
        return encryptedPrivateKey;
    }

    private async Task<string> GetContractBytecodeAsync(string contractType)
    {
        // In production, this would load bytecode from files or configuration
        return contractType switch
        {
            "UserIdentity" => "0x608060405234801561001057600080fd5b50...",
            "Review" => "0x608060405234801561001057600080fd5b50...",
            _ => throw new ArgumentException($"Unknown contract type: {contractType}")
        };
    }

    private string GetUserIdentityContractAbi()
    {
        return @"[
            {
                'constant': false,
                'inputs': [
                    {'name': 'user', 'type': 'address'},
                    {'name': 'dataHash', 'type': 'string'}
                ],
                'name': 'storeIdentity',
                'outputs': [],
                'type': 'function'
            },
            {
                'constant': true,
                'inputs': [
                    {'name': 'user', 'type': 'address'}
                ],
                'name': 'getIdentity',
                'outputs': [{'name': '', 'type': 'string'}],
                'type': 'function'
            }
        ]";
    }

    private string GetReviewContractAbi()
    {
        return @"[
            {
                'constant': false,
                'inputs': [
                    {'name': 'companyId', 'type': 'string'},
                    {'name': 'reviewHash', 'type': 'string'},
                    {'name': 'rating', 'type': 'uint8'}
                ],
                'name': 'createReview',
                'outputs': [],
                'type': 'function'
            },
            {
                'constant': true,
                'inputs': [
                    {'name': 'user', 'type': 'address'}
                ],
                'name': 'getUserReviews',
                'outputs': [{'name': '', 'type': 'string[]'}],
                'type': 'function'
            }
        ]";
    }

    private string GetGenericContractAbi()
    {
        return @"[{'constant':true,'inputs':[],'name':'name','outputs':[{'name':'','type':'string'}],'type':'function'}]";
    }

    private static string GeneratePrivateKey()
    {
        var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
        return ecKey.GetPrivateKey();
    }
}