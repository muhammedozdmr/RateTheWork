namespace RateTheWork.Domain.Interfaces.Policies;

/// <summary>
/// Domain policy base interface'i
/// </summary>
public interface IDomainPolicy
{
    /// <summary>
    /// Policy adı
    /// </summary>
    string PolicyName { get; }
    
    /// <summary>
    /// Policy açıklaması
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Policy aktif mi?
    /// </summary>
    bool IsEnabled { get; }
}
