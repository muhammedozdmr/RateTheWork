namespace RateTheWork.Domain.Interfaces.Events
;

/// <summary>
/// Domain event marker interface'i
/// Tüm domain eventler bu interface'i implemente eder
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Event'in gerçekleştiği zaman
    /// </summary>
    DateTime OccurredOn { get; }
    
    /// <summary>
    /// Event'in benzersiz ID'si
    /// </summary>
    Guid EventId { get; }
}
