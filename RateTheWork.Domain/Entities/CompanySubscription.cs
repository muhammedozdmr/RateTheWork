using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums.Subscription;
using RateTheWork.Domain.Events.CompanySubscription;
using RateTheWork.Domain.Exceptions.SubscriptionException;
using RateTheWork.Domain.ValueObjects.Subscription;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Şirket üyelik bilgileri
/// </summary>
public class CompanySubscription : AuditableBaseEntity
{
    private CompanySubscription() : base()
    {
    }

    // Properties
    public string CompanyId { get; private set; } = string.Empty;
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
    public string? BillingContactId { get; private set; } // Fatura sorumlusu
    public List<FeatureType> Features { get; private set; } = new();
    public Dictionary<string, int> UsageLimits { get; private set; } = new();
    public Dictionary<string, int> CurrentUsage { get; private set; } = new();
    public List<string> AuthorizedHRPersonnelIds { get; private set; } = new(); // Yetkili İK personelleri

    // Ödeme Bilgileri
    public string? InvoiceAddress { get; private set; }
    public string? TaxNumber { get; private set; }
    public string? TaxOffice { get; private set; }
    public bool AutoRenew { get; private set; } = true;

    /// <summary>
    /// Yeni şirket üyeliği oluşturur
    /// </summary>
    public static CompanySubscription Create
    (
        string companyId
        , SubscriptionPlan plan
        , BillingCycle billingCycle
        , string billingContactId
        , bool startTrial = true
    )
    {
        if (string.IsNullOrWhiteSpace(companyId))
            throw new SubscriptionException("Şirket ID boş olamaz");

        if (string.IsNullOrWhiteSpace(billingContactId))
            throw new SubscriptionException("Fatura sorumlusu ID boş olamaz");

        // Sadece şirket planları kabul edilir
        if (plan.Type < SubscriptionType.CompanyBasic)
            throw SubscriptionException.InvalidSubscriptionType(companyId, plan.Type.ToString());

        var subscription = new CompanySubscription
        {
            Id = Guid.NewGuid().ToString(), CompanyId = companyId, Type = plan.Type, StartDate = DateTime.UtcNow
            , BillingCycle = billingCycle, Price = plan.GetPriceForBillingCycle(billingCycle)
            , BillingContactId = billingContactId, IsActive = true, Features = new List<FeatureType>(plan.Features)
            , UsageLimits = new Dictionary<string, int>(plan.Limits), CurrentUsage = new Dictionary<string, int>()
        };

        // Deneme süresi
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

        // İlk İK personeli olarak fatura sorumlusunu ekle
        subscription.AuthorizedHRPersonnelIds.Add(billingContactId);

        subscription.AddDomainEvent(new CompanySubscriptionCreatedEvent(
            subscription.Id,
            companyId,
            plan.Type,
            subscription.StartDate
        ));

        return subscription;
    }

    /// <summary>
    /// Fatura bilgilerini günceller
    /// </summary>
    public void UpdateBillingInfo(string invoiceAddress, string taxNumber, string taxOffice)
    {
        if (string.IsNullOrWhiteSpace(invoiceAddress))
            throw new SubscriptionException("Fatura adresi boş olamaz");

        if (string.IsNullOrWhiteSpace(taxNumber) || taxNumber.Length != 10)
            throw new SubscriptionException("Geçerli bir vergi numarası giriniz");

        InvoiceAddress = invoiceAddress;
        TaxNumber = taxNumber;
        TaxOffice = taxOffice;
    }

    /// <summary>
    /// İK personeli yetkilendirir
    /// </summary>
    public void AuthorizeHRPersonnel(string personnelId)
    {
        if (string.IsNullOrWhiteSpace(personnelId))
            throw new SubscriptionException("Personel ID boş olamaz");

        if (AuthorizedHRPersonnelIds.Contains(personnelId))
            throw new SubscriptionException("Bu personel zaten yetkili");

        // Limit kontrolü
        var hrAccountLimit = GetLimit("HRAccounts");
        if (hrAccountLimit != -1 && AuthorizedHRPersonnelIds.Count >= hrAccountLimit)
            throw SubscriptionException.QuotaExceeded(Id, "HRAccounts", hrAccountLimit);

        AuthorizedHRPersonnelIds.Add(personnelId);
    }

    /// <summary>
    /// İK personeli yetkisini kaldırır
    /// </summary>
    public void RevokeHRPersonnel(string personnelId)
    {
        if (!AuthorizedHRPersonnelIds.Contains(personnelId))
            throw new SubscriptionException("Bu personel yetkili değil");

        // En az bir yetkili kalmalı
        if (AuthorizedHRPersonnelIds.Count == 1)
            throw new SubscriptionException("En az bir yetkili İK personeli olmalı");

        AuthorizedHRPersonnelIds.Remove(personnelId);
    }

    /// <summary>
    /// İK personelinin yetkili olup olmadığını kontrol eder
    /// </summary>
    public bool IsHRPersonnelAuthorized(string personnelId)
    {
        return AuthorizedHRPersonnelIds.Contains(personnelId);
    }

    /// <summary>
    /// İş ilanı yayınlama hakkı olup olmadığını kontrol eder
    /// </summary>
    public bool CanPostJob()
    {
        if (!IsActive || IsExpired())
            return false;

        if (!HasFeature(FeatureType.PostJobListing))
            return false;

        return CheckUsageLimit("MonthlyJobPostings");
    }

    /// <summary>
    /// Yorum yanıtlama hakkı olup olmadığını kontrol eder
    /// </summary>
    public bool CanRespondToReview()
    {
        if (!IsActive || IsExpired())
            return false;

        if (!HasFeature(FeatureType.RespondToReviews))
            return false;

        return CheckUsageLimit("MonthlyResponses");
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

        AddDomainEvent(new CompanySubscriptionUpgradedEvent(
            Id,
            CompanyId,
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
        AutoRenew = false;

        // Mevcut fatura döneminin sonunda iptal olacak
        if (NextBillingDate.HasValue)
        {
            EndDate = NextBillingDate.Value;
        }

        AddDomainEvent(new CompanySubscriptionCancelledEvent(
            Id,
            CompanyId,
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

        NextBillingDate = CalculateNextBillingDate(NextBillingDate ?? DateTime.UtcNow, BillingCycle);

        // İptal edildiyse iptal durumunu kaldır
        if (IsCancelled)
        {
            IsCancelled = false;
            CancelledAt = null;
            CancellationReason = null;
            EndDate = null;
            AutoRenew = true;
        }

        AddDomainEvent(new CompanySubscriptionRenewedEvent(
            Id,
            CompanyId,
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
            return true;

        if (limit == -1)
            return true; // Sınırsız

        if (!CurrentUsage.TryGetValue(feature, out var usage))
            return true;

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
        return EndDate.HasValue && EndDate.Value < DateTime.UtcNow;
    }

    /// <summary>
    /// Özelliğin kullanılabilir olup olmadığını kontrol eder
    /// </summary>
    public bool HasFeature(FeatureType feature)
    {
        return Features.Contains(feature);
    }

    /// <summary>
    /// Limiti döndürür
    /// </summary>
    public int GetLimit(string limitName)
    {
        return UsageLimits.TryGetValue(limitName, out var limit) ? limit : 0;
    }

    /// <summary>
    /// Kalan kullanım hakkını döndürür
    /// </summary>
    public int GetRemainingUsage(string feature)
    {
        var limit = GetLimit(feature);
        if (limit == -1) return -1; // Sınırsız

        var usage = CurrentUsage.TryGetValue(feature, out var u) ? u : 0;
        return Math.Max(0, limit - usage);
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
}
