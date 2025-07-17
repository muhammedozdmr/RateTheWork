using RateTheWork.Domain.ValueObjects.Common;

namespace RateTheWork.Domain.ValueObjects.User;

/// <summary>
/// Rozet ilerleme durumu
/// </summary>
public sealed class BadgeProgress : ValueObject
{
    private BadgeProgress
    (
        string badgeId
        , int currentProgress
        , int targetValue
        , DateTime? completedAt = null
    )
    {
        BadgeId = badgeId ?? throw new ArgumentNullException(nameof(badgeId));
        CurrentProgress = Math.Max(0, currentProgress);
        TargetValue = Math.Max(1, targetValue);
        CompletedAt = completedAt;
        IsCompleted = CurrentProgress >= TargetValue;

        ProgressPercentage = Math.Min(100, Math.Round((decimal)CurrentProgress / TargetValue * 100, 2));
    }

    /// <summary>
    /// Rozet ID'si
    /// </summary>
    public string BadgeId { get; }

    /// <summary>
    /// Mevcut ilerleme
    /// </summary>
    public int CurrentProgress { get; }

    /// <summary>
    /// Hedef değer
    /// </summary>
    public int TargetValue { get; }

    /// <summary>
    /// İlerleme yüzdesi
    /// </summary>
    public decimal ProgressPercentage { get; }

    /// <summary>
    /// Tamamlanma tarihi
    /// </summary>
    public DateTime? CompletedAt { get; }

    /// <summary>
    /// Tamamlandı mı?
    /// </summary>
    public bool IsCompleted { get; }

    /// <summary>
    /// Rozet ilerlemesi oluşturur
    /// </summary>
    public static BadgeProgress Create(string badgeId, int currentProgress, int targetValue)
    {
        if (string.IsNullOrEmpty(badgeId))
            throw new ArgumentException("Badge ID boş olamaz", nameof(badgeId));

        var completedAt = currentProgress >= targetValue ? DateTime.UtcNow : (DateTime?)null;
        return new BadgeProgress(badgeId, currentProgress, targetValue, completedAt);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return BadgeId;
        yield return CurrentProgress;
        yield return TargetValue;
        yield return CompletedAt ?? DateTime.MinValue;
    }
}
