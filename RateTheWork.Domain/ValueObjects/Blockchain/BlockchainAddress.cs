using System.Text.RegularExpressions;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.ValueObjects.Common;

namespace RateTheWork.Domain.ValueObjects.Blockchain;

public sealed class BlockchainAddress : ValueObject
{
    private const string EthereumAddressPattern = @"^0x[a-fA-F0-9]{40}$";
    
    public string Value { get; }
    public BlockchainNetworkType NetworkType { get; }
    
    private BlockchainAddress(string value, BlockchainNetworkType networkType)
    {
        Value = value;
        NetworkType = networkType;
    }
    
    public static BlockchainAddress Create(string address, BlockchainNetworkType networkType = BlockchainNetworkType.Ethereum)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new DomainValidationException("address", "Blockchain address cannot be empty");
            
        address = address.Trim();
        
        if (!IsValidAddress(address, networkType))
            throw new DomainValidationException("address", $"Invalid {networkType} blockchain address format");
            
        return new BlockchainAddress(address.ToLowerInvariant(), networkType);
    }
    
    private static bool IsValidAddress(string address, BlockchainNetworkType networkType)
    {
        return networkType switch
        {
            BlockchainNetworkType.Ethereum => Regex.IsMatch(address, EthereumAddressPattern),
            BlockchainNetworkType.BinanceSmartChain => Regex.IsMatch(address, EthereumAddressPattern),
            BlockchainNetworkType.Polygon => Regex.IsMatch(address, EthereumAddressPattern),
            _ => false
        };
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return NetworkType;
    }
    
    public override string ToString() => Value;
}

public enum BlockchainNetworkType
{
    Ethereum,
    BinanceSmartChain,
    Polygon
}