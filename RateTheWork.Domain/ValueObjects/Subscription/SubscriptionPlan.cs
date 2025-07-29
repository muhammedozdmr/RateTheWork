using RateTheWork.Domain.Enums.Subscription;
using RateTheWork.Domain.ValueObjects.Common;

namespace RateTheWork.Domain.ValueObjects.Subscription;

/// <summary>
/// Üyelik planı detayları
/// </summary>
public sealed class SubscriptionPlan : ValueObject
{
    private SubscriptionPlan
    (
        SubscriptionType type
        , string name
        , string description
        , decimal monthlyPrice
        , decimal annualPrice
        , List<FeatureType> features
        , Dictionary<string, int> limits
        , int trialDays = 0
        , bool isActive = true
    )
    {
        Type = type;
        Name = name;
        Description = description;
        MonthlyPrice = monthlyPrice;
        AnnualPrice = annualPrice;
        Features = features;
        Limits = limits;
        TrialDays = trialDays;
        IsActive = isActive;
    }

    public SubscriptionType Type { get; }
    public string Name { get; }
    public string Description { get; }
    public decimal MonthlyPrice { get; }
    public decimal AnnualPrice { get; }
    public List<FeatureType> Features { get; }
    public Dictionary<string, int> Limits { get; } // Örn: "JobPostings" -> 5
    public int TrialDays { get; }
    public bool IsActive { get; }

    public static SubscriptionPlan CreateFreePlan()
    {
        return new SubscriptionPlan(
            SubscriptionType.Free,
            "Ücretsiz Plan",
            "İlk 6 ay tüm kullanıcılar için ücretsiz",
            0,
            0,
            new List<FeatureType>
            {
                FeatureType.CreateReview, FeatureType.UnlimitedReviewReading, FeatureType.AdvancedSearch
                , FeatureType.CompanyFollowing, FeatureType.AnonymousReview
            },
            new Dictionary<string, int>
            {
                ["DailyReviews"] = 5, ["CompanyFollows"] = 10
            },
            180 // 6 ay
        );
    }

    public static SubscriptionPlan CreateIndividualPremium()
    {
        return new SubscriptionPlan(
            SubscriptionType.IndividualPremium,
            "Bireysel Premium",
            "Gelişmiş özellikler ve sınırsız erişim",
            49.99m,
            479.99m, // Yıllıkta 2 ay ücretsiz
            new List<FeatureType>
            {
                FeatureType.CreateReview, FeatureType.UnlimitedReviewReading, FeatureType.ViewDetailedAnalytics
                , FeatureType.AdvancedSearch, FeatureType.ReviewNotifications, FeatureType.CompanyFollowing
                , FeatureType.ViewSalaryInfo, FeatureType.AnonymousReview
            },
            new Dictionary<string, int>
            {
                ["DailyReviews"] = -1, // Sınırsız
                ["CompanyFollows"] = -1 // Sınırsız
            }
        );
    }

    public static SubscriptionPlan CreateCompanyBasic()
    {
        return new SubscriptionPlan(
            SubscriptionType.CompanyBasic,
            "Şirket Temel",
            "Küçük işletmeler için temel özellikler",
            299.99m,
            2999.99m,
            new List<FeatureType>
            {
                FeatureType.PostJobListing, FeatureType.ViewCompanyReviews, FeatureType.RespondToReviews
                , FeatureType.HRVerification
            },
            new Dictionary<string, int>
            {
                ["MonthlyJobPostings"] = 5, ["HRAccounts"] = 1, ["MonthlyResponses"] = 20
            },
            14 // 14 gün deneme
        );
    }

    public static SubscriptionPlan CreateCompanyProfessional()
    {
        return new SubscriptionPlan(
            SubscriptionType.CompanyProfessional,
            "Şirket Profesyonel",
            "Orta ölçekli işletmeler için gelişmiş özellikler",
            799.99m,
            7999.99m,
            new List<FeatureType>
            {
                FeatureType.PostJobListing, FeatureType.ViewCompanyReviews, FeatureType.ReviewAnalytics
                , FeatureType.RespondToReviews, FeatureType.HRVerification, FeatureType.ApplicantAnalytics
                , FeatureType.JobPostingAnalytics, FeatureType.MultipleHRAccounts
            },
            new Dictionary<string, int>
            {
                ["MonthlyJobPostings"] = 20, ["HRAccounts"] = 5, ["MonthlyResponses"] = -1 // Sınırsız
            },
            30 // 30 gün deneme
        );
    }

    public static SubscriptionPlan CreateCompanyEnterprise()
    {
        return new SubscriptionPlan(
            SubscriptionType.CompanyEnterprise,
            "Şirket Kurumsal",
            "Büyük işletmeler için özel çözümler",
            1999.99m,
            19999.99m,
            new List<FeatureType>
            {
                FeatureType.PostJobListing, FeatureType.ViewCompanyReviews, FeatureType.ReviewAnalytics
                , FeatureType.RespondToReviews, FeatureType.HRVerification, FeatureType.ApplicantAnalytics
                , FeatureType.APIAccess, FeatureType.CustomReporting, FeatureType.MultipleHRAccounts
                , FeatureType.JobPostingAnalytics, FeatureType.CompetitorAnalysis, FeatureType.BrandManagement
                , FeatureType.PrioritySupport
            },
            new Dictionary<string, int>
            {
                ["MonthlyJobPostings"] = -1, // Sınırsız
                ["HRAccounts"] = -1
                , // Sınırsız
                ["MonthlyResponses"] = -1
                , // Sınırsız
                ["APICallsPerDay"] = 10000
            },
            30 // 30 gün deneme
        );
    }

    public bool HasFeature(FeatureType feature)
    {
        return Features.Contains(feature);
    }

    public int GetLimit(string limitName)
    {
        return Limits.TryGetValue(limitName, out var limit) ? limit : 0;
    }

    public bool IsUnlimited(string limitName)
    {
        return GetLimit(limitName) == -1;
    }

    public decimal GetPriceForBillingCycle(BillingCycle cycle)
    {
        return cycle switch
        {
            BillingCycle.Monthly => MonthlyPrice, BillingCycle.Quarterly => MonthlyPrice * 3 * 0.95m, // %5 indirim
            BillingCycle.SemiAnnual => MonthlyPrice * 6 * 0.90m
            , // %10 indirim
            BillingCycle.Annual => AnnualPrice
            , _ => MonthlyPrice
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return Name;
        yield return MonthlyPrice;
        yield return AnnualPrice;
    }
    
    /// <summary>
    /// Plan tipine göre plan detaylarını getirir
    /// </summary>
    public static SubscriptionPlan GetPlan(SubscriptionType type)
    {
        return type switch
        {
            SubscriptionType.Free => CreateFreePlan(),
            SubscriptionType.IndividualPremium => CreateIndividualPremium(),
            SubscriptionType.CompanyBasic => CreateCompanyBasic(),
            SubscriptionType.CompanyProfessional => CreateCompanyProfessional(),
            SubscriptionType.CompanyEnterprise => CreateCompanyEnterprise(),
            _ => throw new ArgumentException($"Unknown subscription type: {type}")
        };
    }
}
