namespace RateTheWork.Application.Common.Exceptions;

/// <summary>
/// İş kuralı ihlallerini temsil eden exception
/// </summary>
public class BusinessRuleException : Exception
{
    /// <summary>
    /// İhlal edilen kural kodu
    /// </summary>
    public string? RuleCode { get; }

    /// <summary>
    /// BusinessRuleException oluşturur
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    public BusinessRuleException(string message) : base(message)
    {
    }

    /// <summary>
    /// Kural kodu ile BusinessRuleException oluşturur
    /// </summary>
    /// <param name="ruleCode">Kural kodu</param>
    /// <param name="message">Hata mesajı</param>
    public BusinessRuleException(string ruleCode, string message) : base(message)
    {
        RuleCode = ruleCode;
    }
}

