namespace RateTheWork.Domain.Exceptions.ConcurrencyException;

/// <summary>
/// Optimistic concurrency exception'ı
/// </summary>
public class OptimisticConcurrencyException : DomainException
{
    public OptimisticConcurrencyException
    (
        string entityType
        , string entityId
        , int expectedVersion
        , int currentVersion
        , string? modifiedBy = null
        , DateTime? lastModified = null
    )
        : base(
            $"Optimistic concurrency conflict for {entityType} '{entityId}'. Expected version: {expectedVersion}, Current version: {currentVersion}")
    {
        EntityType = entityType;
        EntityId = entityId;
        ExpectedVersion = expectedVersion;
        CurrentVersion = currentVersion;
        ModifiedBy = modifiedBy;
        LastModified = lastModified;
    }

    /// <summary>
    /// Entity tipi
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Entity ID'si
    /// </summary>
    public string EntityId { get; }

    /// <summary>
    /// Beklenen versiyon
    /// </summary>
    public int ExpectedVersion { get; }

    /// <summary>
    /// Mevcut versiyon
    /// </summary>
    public int CurrentVersion { get; }

    /// <summary>
    /// Değiştiren kullanıcı
    /// </summary>
    public string? ModifiedBy { get; }

    /// <summary>
    /// Son değişiklik zamanı
    /// </summary>
    public DateTime? LastModified { get; }
}
