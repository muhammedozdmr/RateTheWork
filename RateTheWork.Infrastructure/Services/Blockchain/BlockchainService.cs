using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using RateTheWork.Domain.Interfaces.Blockchain;
using RateTheWork.Domain.ValueObjects.Blockchain;
using Nethereum.Hex.HexConvertors.Extensions;

namespace RateTheWork.Infrastructure.Services.Blockchain;

public class BlockchainService : IBlockchainService
{
    private readonly Web3 _web3;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BlockchainService> _logger;
    private readonly string _contractAddress;
    private readonly Account _adminAccount;

    public BlockchainService(
        IConfiguration configuration,
        ILogger<BlockchainService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var rpcUrl = _configuration["Blockchain:RpcUrl"] ?? "http://localhost:8545";
        var privateKey = _configuration["Blockchain:AdminPrivateKey"] ?? GeneratePrivateKey();
        _contractAddress = _configuration["Blockchain:ContractAddress"] ?? "";

        _adminAccount = new Account(privateKey);
        _web3 = new Web3(_adminAccount, rpcUrl);
    }

    public async Task<UserBlockchainIdentity> CreateUserIdentityAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Yeni wallet oluştur
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
            var publicKey = ecKey.GetPubKey().ToHex();
            var address = ecKey.GetPublicAddress();

            // Private key'i şifrele
            var encryptedPrivateKey = await EncryptPrivateKeyAsync(privateKey, userId.ToString());

            // Blockchain address oluştur
            var walletAddress = BlockchainAddress.Create(address);

            // Identity oluştur
            var identity = UserBlockchainIdentity.Create(
                walletAddress,
                encryptedPrivateKey,
                publicKey);

            _logger.LogInformation("Blockchain identity created for user {UserId} with address {Address}",
                userId, address);

            return identity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating blockchain identity for user {UserId}", userId);
            throw;
        }
    }

    public async Task<BlockchainTransaction> StoreUserDataHashAsync(
        UserBlockchainIdentity identity,
        string dataHash,
        string dataType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Smart contract'a veri hash'ini kaydet
            var contract = _web3.Eth.GetContract(GetUserDataContractAbi(), _contractAddress);
            var storeFunction = contract.GetFunction("storeDataHash");

            // Private key'i decrypt et
            var privateKey = await DecryptPrivateKeyAsync(
                identity.EncryptedPrivateKey,
                identity.WalletAddress.Value);

            var account = new Account(privateKey);
            var web3 = new Web3(account, _web3.Client);

            // Transaction gönder
            var receipt = await storeFunction.SendTransactionAndWaitForReceiptAsync(
                account.Address,
                new HexBigInteger(900000),
                new HexBigInteger(0),
                null,
                dataType,
                dataHash);

            // Transaction oluştur
            var transaction = BlockchainTransaction.Create(
                receipt.TransactionHash,
                identity.WalletAddress,
                BlockchainAddress.Create(_contractAddress),
                (long)receipt.BlockNumber.Value,
                dataHash);

            if (receipt.Status?.Value == 1)
            {
                transaction = transaction.MarkAsConfirmed();
            }
            else
            {
                transaction = transaction.MarkAsFailed();
            }

            _logger.LogInformation("User data hash stored on blockchain: {TransactionHash}", receipt.TransactionHash);

            return transaction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing user data hash on blockchain");
            throw;
        }
    }

    public async Task<string> RetrieveUserDataHashAsync(
        BlockchainAddress walletAddress,
        string dataType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contract = _web3.Eth.GetContract(GetUserDataContractAbi(), _contractAddress);
            var retrieveFunction = contract.GetFunction("getDataHash");

            var result = await retrieveFunction.CallAsync<string>(
                walletAddress.Value,
                dataType);

            return result ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user data hash from blockchain");
            throw;
        }
    }

    public async Task<bool> VerifyUserIdentityAsync(
        BlockchainAddress walletAddress,
        string signature,
        string message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var signer = new Nethereum.Signer.MessageSigner();
            var recoveredAddress = signer.EcRecover(Encoding.UTF8.GetBytes(message), signature);

            return string.Equals(recoveredAddress, walletAddress.Value, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying user identity");
            return false;
        }
    }

    public async Task<BlockchainTransaction> CreateAnonymousReviewAsync(
        UserBlockchainIdentity identity,
        Guid companyId,
        string reviewHash,
        int rating,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contract = _web3.Eth.GetContract(GetReviewContractAbi(), _contractAddress);
            var createReviewFunction = contract.GetFunction("createReview");

            // Private key'i decrypt et
            var privateKey = await DecryptPrivateKeyAsync(
                identity.EncryptedPrivateKey,
                identity.WalletAddress.Value);

            var account = new Account(privateKey);
            var web3 = new Web3(account, _web3.Client);

            // Transaction gönder
            var receipt = await createReviewFunction.SendTransactionAndWaitForReceiptAsync(
                account.Address,
                new HexBigInteger(900000),
                new HexBigInteger(0),
                null,
                companyId.ToString(),
                reviewHash,
                rating);

            // Transaction oluştur
            var transaction = BlockchainTransaction.Create(
                receipt.TransactionHash,
                identity.WalletAddress,
                BlockchainAddress.Create(_contractAddress),
                (long)receipt.BlockNumber.Value,
                reviewHash);

            if (receipt.Status?.Value == 1)
            {
                transaction = transaction.MarkAsConfirmed();
            }
            else
            {
                transaction = transaction.MarkAsFailed();
            }

            _logger.LogInformation("Anonymous review created on blockchain: {TransactionHash}", receipt.TransactionHash);

            return transaction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating anonymous review on blockchain");
            throw;
        }
    }

    public async Task<List<string>> GetUserReviewHashesAsync(
        BlockchainAddress walletAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contract = _web3.Eth.GetContract(GetReviewContractAbi(), _contractAddress);
            var getReviewsFunction = contract.GetFunction("getUserReviews");

            var result = await getReviewsFunction.CallAsync<List<string>>(walletAddress.Value);

            return result ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user review hashes from blockchain");
            throw;
        }
    }

    public async Task<bool> VerifyReviewOwnershipAsync(
        BlockchainAddress walletAddress,
        string reviewHash,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contract = _web3.Eth.GetContract(GetReviewContractAbi(), _contractAddress);
            var verifyFunction = contract.GetFunction("verifyReviewOwnership");

            var result = await verifyFunction.CallAsync<bool>(
                walletAddress.Value,
                reviewHash);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying review ownership on blockchain");
            return false;
        }
    }

    public async Task<BlockchainAddress> GenerateNewWalletAddressAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            var address = ecKey.GetPublicAddress();

            return BlockchainAddress.Create(address);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating new wallet address");
            throw;
        }
    }

    public async Task<string> EncryptPrivateKeyAsync(string privateKey, string passphrase)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Passphrase'den key ve IV oluştur
            var key = GenerateKeyFromPassphrase(passphrase, 32);
            var iv = GenerateKeyFromPassphrase(passphrase + "_iv", 16);

            aes.Key = key;
            aes.IV = iv;

            using var encryptor = aes.CreateEncryptor();
            var privateKeyBytes = Encoding.UTF8.GetBytes(privateKey);
            var encryptedBytes = encryptor.TransformFinalBlock(privateKeyBytes, 0, privateKeyBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting private key");
            throw;
        }
    }

    public async Task<string> DecryptPrivateKeyAsync(string encryptedPrivateKey, string passphrase)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Passphrase'den key ve IV oluştur
            var key = GenerateKeyFromPassphrase(passphrase, 32);
            var iv = GenerateKeyFromPassphrase(passphrase + "_iv", 16);

            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var encryptedBytes = Convert.FromBase64String(encryptedPrivateKey);
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting private key");
            throw;
        }
    }

    private static byte[] GenerateKeyFromPassphrase(string passphrase, int keySize)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(passphrase));
        
        var key = new byte[keySize];
        Array.Copy(hash, key, Math.Min(hash.Length, keySize));
        
        return key;
    }

    private static string GeneratePrivateKey()
    {
        var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
        return ecKey.GetPrivateKey();
    }

    private string GetUserDataContractAbi()
    {
        // Basit bir ABI örneği - gerçek projede dosyadan okunmalı
        return @"[
            {
                'constant': false,
                'inputs': [
                    {'name': 'dataType', 'type': 'string'},
                    {'name': 'dataHash', 'type': 'string'}
                ],
                'name': 'storeDataHash',
                'outputs': [],
                'type': 'function'
            },
            {
                'constant': true,
                'inputs': [
                    {'name': 'user', 'type': 'address'},
                    {'name': 'dataType', 'type': 'string'}
                ],
                'name': 'getDataHash',
                'outputs': [{'name': '', 'type': 'string'}],
                'type': 'function'
            }
        ]";
    }

    private string GetReviewContractAbi()
    {
        // Basit bir ABI örneği - gerçek projede dosyadan okunmalı
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
            },
            {
                'constant': true,
                'inputs': [
                    {'name': 'user', 'type': 'address'},
                    {'name': 'reviewHash', 'type': 'string'}
                ],
                'name': 'verifyReviewOwnership',
                'outputs': [{'name': '', 'type': 'bool'}],
                'type': 'function'
            }
        ]";
    }
}