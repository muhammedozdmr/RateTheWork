namespace RateTheWork.Domain.Exceptions;

/// <summary>
/// İş kuralı ihlali exception'ı
/// </summary>
public class BusinessRuleException : DomainException
{
    public string Code { get; }
    public string Details { get; }

    public BusinessRuleException(string message) 
        : base(message)
    {
        Code = "BUSINESS_RULE_VIOLATION";
        Details = message;
    }

    public BusinessRuleException(string code, string message) 
        : base(message)
    {
        Code = code;
        Details = message;
    }

    public BusinessRuleException(string code, string message, string details) 
        : base(message)
    {
        Code = code;
        Details = details;
    }
}