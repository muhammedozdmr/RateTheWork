namespace RateTheWork.Domain.Exceptions.SecureException;

/// <summary>
/// IP adresi kısıtlama exception'ı
/// </summary>
public class IpAddressRestrictionException : DomainException
{
    public string IpAddress { get; }
    public string RestrictionType { get; }
    public string Reason { get; }

    public IpAddressRestrictionException(string ipAddress, string restrictionType, string reason)
        : base($"IP address '{ipAddress}' is restricted. Type: {restrictionType}, Reason: {reason}")
    {
        IpAddress = ipAddress;
        RestrictionType = restrictionType;
        Reason = reason;
    }
}
