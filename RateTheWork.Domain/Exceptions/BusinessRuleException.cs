namespace RateTheWork.Domain.Exceptions;

/// <summary>
/// İş kuralı ihlali exception'ı
/// </summary>
public class BusinessRuleException : DomainException
{
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

    /// <summary>
    /// Entity bilgisi ile exception oluştur
    /// </summary>
    public BusinessRuleException(string code, string message, string entityType, string entityId)
        : base(message)
    {
        Code = code;
        Details = message;
        EntityType = entityType;
        EntityId = entityId;
    }

    /// <summary>
    /// Hata kodu
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Detaylı açıklama
    /// </summary>
    public string Details { get; }

    /// <summary>
    /// İhlal edilen kural
    /// </summary>
    public string? ViolatedRule { get; private set; }

    /// <summary>
    /// İlgili entity tipi
    /// </summary>
    public string? EntityType { get; }

    /// <summary>
    /// İlgili entity ID'si
    /// </summary>
    public string? EntityId { get; }

    /// <summary>
    /// Kural bilgisi ekle
    /// </summary>
    public BusinessRuleException WithRule(string violatedRule)
    {
        ViolatedRule = violatedRule;
        return this;
    }
}
