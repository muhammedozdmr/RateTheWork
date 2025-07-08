namespace RateTheWork.Domain.Exceptions.SecureException;

/// <summary>
/// Güvenlik tokenı geçersiz exception'ı
/// </summary>
public class InvalidSecurityTokenException : DomainException
{
    public string TokenType { get; }
    public string InvalidationReason { get; }

    public InvalidSecurityTokenException(string tokenType, string invalidationReason)
        : base($"Security token of type '{tokenType}' is invalid. Reason: {invalidationReason}")
    {
        TokenType = tokenType;
        InvalidationReason = invalidationReason;
    }
}

