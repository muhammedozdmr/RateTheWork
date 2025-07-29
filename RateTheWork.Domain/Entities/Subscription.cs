using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums.Subscription;
using RateTheWork.Domain.Events.Subscription;
using RateTheWork.Domain.Exceptions.SubscriptionException;
using RateTheWork.Domain.ValueObjects.Subscription;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Kullanıcı üyelik bilgileri
/// </summary>
public class Subscription : AuditableBaseEntity
{
    private Subscription() : base()
    {
    }

    // Properties
    public string UserId { get; private set; } = string.Empty;
    public SubscriptionType Type { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? NextBillingDate { get; private set; }
    public BillingCycle BillingCycle { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "TRY";
    public bool IsActive { get; private set; }
    public bool IsCancelled { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public bool IsTrialPeriod { get; private set; }
    public DateTime? TrialEndDate { get; private set; }
    public string? PaymentMethodId { get; private set; }
    public List<FeatureType> Features { get; private set; } = new();
    public Dictionary<string, int> UsageLimits { get; private set; } = new();
    public Dictionary<string, int> CurrentUsage { get; private set; } = new();

    /// <summary>
    /// Yeni üyelik oluşturur
    /// </summary>
    public static Subscription Create
    (
        string userId
        , SubscriptionPlan plan
        , BillingCycle billingCycle
        , bool startTrial = false
    )
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new SubscriptionException("Kullanıcı ID boş olamaz");

        var subscription = new Subscription
        {
            Id = Guid.NewGuid().ToString(), UserId = userId, Type = plan.Type, StartDate = DateTime.UtcNow
            , BillingCycle = billingCycle, Price = plan.GetPriceForBillingCycle(billingCycle), IsActive = true
            , Features = new List<FeatureType>(plan.Features), UsageLimits = new Dictionary<string, int>(plan.Limits)
        };

        if (startTrial && plan.TrialDays > 0)
        {
            subscription.IsTrialPeriod = true;
            subscription.TrialEndDate = DateTime.UtcNow.AddDays(plan.TrialDays);
            subscription.NextBillingDate = subscription.TrialEndDate;
        }
        else
        {
            subscription.NextBillingDate = CalculateNextBillingDate(DateTime.UtcNow, billingCycle);
        }

        // İlk 6 ay ücretsiz kampanya kontrolü
        if (plan.Type == SubscriptionType.Free)
        {
            subscription.EndDate = DateTime.UtcNow.AddMonths(6);
            subscription.Price = 0;
        }

        subscription.AddDomainEvent(new SubscriptionCreatedEvent(
            subscription.Id,
            userId,
            plan.Type,
            subscription.StartDate
        ));

        return subscription;
    }

    /// <summary>
    /// Üyeliği yükseltir
    /// </summary>
    public void Upgrade(SubscriptionPlan newPlan, BillingCycle newBillingCycle)
    {
        if (!IsActive)
            throw new SubscriptionException("Aktif olmayan üyelik yükseltilemez");

        if (newPlan.Type <= Type)
            throw new SubscriptionException("Yeni plan mevcut plandan düşük veya aynı olamaz");

        var oldType = Type;
        Type = newPlan.Type;
        BillingCycle = newBillingCycle;
        Price = newPlan.GetPriceForBillingCycle(newBillingCycle);
        Features = new List<FeatureType>(newPlan.Features);
        UsageLimits = new Dictionary<string, int>(newPlan.Limits);

        // Deneme süresi bittiyse normal faturalamaya geç
        if (IsTrialPeriod && TrialEndDate < DateTime.UtcNow)
        {
            IsTrialPeriod = false;
            TrialEndDate = null;
        }

        AddDomainEvent(new SubscriptionUpgradedEvent(
            Id,
            UserId,
            oldType,
            Type,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Üyeliği iptal eder
    /// </summary>
    public void Cancel(string reason)
    {
        if (!IsActive)
            throw SubscriptionException.CannotCancel(Id, "Subscription is not active");

        IsCancelled = true;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;

        // Mevcut fatura döneminin sonunda iptal olacak
        if (NextBillingDate.HasValue)
        {
            EndDate = NextBillingDate.Value;
        }

        AddDomainEvent(new SubscriptionCancelledEvent(
            Id,
            UserId,
            Type,
            CancelledAt.Value,
            reason
        ));
    }

    /// <summary>
    /// Üyeliği yeniler
    /// </summary>
    public void Renew()
    {
        if (!IsActive)
            throw new SubscriptionException("Aktif olmayan üyelik yenilenemez");

        if (Type == SubscriptionType.Free)
            throw new SubscriptionException("Ücretsiz üyelik yenilenemez");

        NextBillingDate = CalculateNextBillingDate(NextBillingDate ?? DateTime.UtcNow, BillingCycle);

        // İptal edildiyse iptal durumunu kaldır
        if (IsCancelled)
        {
            IsCancelled = false;
            CancelledAt = null;
            CancellationReason = null;
            EndDate = null;
        }

        AddDomainEvent(new SubscriptionRenewedEvent(
            Id,
            UserId,
            Type,
            NextBillingDate.Value
        ));
    }

    /// <summary>
    /// Deneme süresini sonlandırır
    /// </summary>
    public void EndTrial()
    {
        if (!IsTrialPeriod)
            return;

        IsTrialPeriod = false;
        TrialEndDate = DateTime.UtcNow;
        NextBillingDate = CalculateNextBillingDate(DateTime.UtcNow, BillingCycle);
    }

    /// <summary>
    /// Kullanım limitini kontrol eder
    /// </summary>
    public bool CheckUsageLimit(string feature)
    {
        if (!UsageLimits.TryGetValue(feature, out var limit))
            return true; // Limit tanımlı değilse sınırsız kabul et

        if (limit == -1)
            return true; // Sınırsız

        if (!CurrentUsage.TryGetValue(feature, out var usage))
            return true; // Henüz kullanılmamış

        return usage < limit;
    }

    /// <summary>
    /// Kullanımı artırır
    /// </summary>
    public void IncrementUsage(string feature)
    {
        if (!CurrentUsage.ContainsKey(feature))
            CurrentUsage[feature] = 0;

        CurrentUsage[feature]++;
    }

    /// <summary>
    /// Aylık kullanımları sıfırlar
    /// </summary>
    public void ResetMonthlyUsage()
    {
        var monthlyFeatures = new[] { "MonthlyJobPostings", "MonthlyResponses" };

        foreach (var feature in monthlyFeatures)
        {
            if (CurrentUsage.ContainsKey(feature))
                CurrentUsage[feature] = 0;
        }
    }

    /// <summary>
    /// Üyeliğin süresinin dolup dolmadığını kontrol eder
    /// </summary>
    public bool IsExpired()
    {
        if (EndDate.HasValue && EndDate.Value < DateTime.UtcNow)
            return true;

        if (Type == SubscriptionType.Free && StartDate.AddMonths(6) < DateTime.UtcNow)
            return true;

        return false;
    }

    /// <summary>
    /// Özelliğin kullanılabilir olup olmadığını kontrol eder
    /// </summary>
    public bool HasFeature(FeatureType feature)
    {
        return Features.Contains(feature);
    }

    private static DateTime CalculateNextBillingDate(DateTime fromDate, BillingCycle cycle)
    {
        return cycle switch
        {
            BillingCycle.Monthly => fromDate.AddMonths(1), BillingCycle.Quarterly => fromDate.AddMonths(3)
            , BillingCycle.SemiAnnual => fromDate.AddMonths(6), BillingCycle.Annual => fromDate.AddYears(1)
            , _ => fromDate.AddMonths(1)
        };
    }
    
    /// <summary>
    /// Ödeme yöntemini ayarlar
    /// </summary>
    public void SetPaymentMethod(string paymentMethodId)
    {
        if (string.IsNullOrWhiteSpace(paymentMethodId))
            throw new ArgumentException("Ödeme yöntemi ID'si boş olamaz", nameof(paymentMethodId));
            
        PaymentMethodId = paymentMethodId;
        ModifiedAt = DateTime.UtcNow;
    }
}
