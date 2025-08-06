namespace RateTheWork.Infrastructure.Configuration;

public class BlockchainConfiguration
{
    public const string SectionName = "Blockchain";
    
    public string RpcUrl { get; set; } = "http://localhost:8545";
    public string AdminPrivateKey { get; set; } = string.Empty;
    public string ContractAddress { get; set; } = string.Empty;
    public string UserIdentityContractAddress { get; set; } = string.Empty;
    public string ReviewContractAddress { get; set; } = string.Empty;
    public string NetworkType { get; set; } = "Local";
    public int ChainId { get; set; } = 1337;
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelayMilliseconds { get; set; } = 1000;
    public bool EnableTransactionLogging { get; set; } = true;
    public decimal GasMultiplier { get; set; } = 1.2m;
    public long DefaultGasLimit { get; set; } = 900000;
    public bool UseInfuraProvider { get; set; } = false;
    public string InfuraProjectId { get; set; } = string.Empty;
    public string InfuraProjectSecret { get; set; } = string.Empty;
}