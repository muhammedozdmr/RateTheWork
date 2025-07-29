using AutoMapper;
using RateTheWork.Application.Features.Subscriptions.Queries.GetUserSubscription;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.Subscription;

namespace RateTheWork.Application.Common.Mappings;

/// <summary>
/// Subscription entity mapping profili
/// </summary>
public class SubscriptionMappingProfile : Profile
{
    public SubscriptionMappingProfile()
    {
        // Subscription -> SubscriptionDto
        CreateMap<Subscription, SubscriptionDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => new SubscriptionStatusDto
            {
                IsExpired = !src.IsActive && src.EndDate.HasValue && src.EndDate.Value < DateTime.UtcNow
                , IsInTrial = src.IsTrialPeriod && src.TrialEndDate.HasValue && src.TrialEndDate.Value > DateTime.UtcNow
                , IsGracePeriod = src.IsCancelled && src.EndDate.HasValue && src.EndDate.Value > DateTime.UtcNow
                , DaysUntilExpiry = src.EndDate.HasValue ? (int)(src.EndDate.Value - DateTime.UtcNow).TotalDays : 0
                , DaysUntilRenewal = src.NextBillingDate.HasValue
                    ? (int)(src.NextBillingDate.Value - DateTime.UtcNow).TotalDays
                    : 0
            }));

        // CompanySubscription -> CompanySubscriptionDto
        CreateMap<CompanySubscription, CompanySubscriptionDto>()
            .ForMember(dest => dest.MaxJobPostings, 
                opt => opt.MapFrom(src => src.UsageLimits.ContainsKey("MaxJobPostings") ? src.UsageLimits["MaxJobPostings"] : 0))
            .ForMember(dest => dest.ActiveJobPostings, 
                opt => opt.MapFrom(src => src.CurrentUsage.ContainsKey("JobPostings") ? src.CurrentUsage["JobPostings"] : 0))
            .ForMember(dest => dest.RemainingJobPostings,
                opt => opt.MapFrom(src => 
                    (src.UsageLimits.ContainsKey("MaxJobPostings") ? src.UsageLimits["MaxJobPostings"] : 0) -
                    (src.CurrentUsage.ContainsKey("JobPostings") ? src.CurrentUsage["JobPostings"] : 0)))
            .ForMember(dest => dest.MaxHRPersonnel,
                opt => opt.MapFrom(src => src.UsageLimits.ContainsKey("MaxHRPersonnel") ? src.UsageLimits["MaxHRPersonnel"] : 0))
            .ForMember(dest => dest.RemainingHRPersonnel,
                opt => opt.MapFrom(src => 
                    (src.UsageLimits.ContainsKey("MaxHRPersonnel") ? src.UsageLimits["MaxHRPersonnel"] : 0) -
                    src.AuthorizedHRPersonnelIds.Count))
            .ForMember(dest => dest.AuthorizedPersonnelIds, opt => opt.MapFrom(src => src.AuthorizedHRPersonnelIds))
            .ForMember(dest => dest.CanPostJobs, opt => opt.MapFrom(src => src.Features.Contains(FeatureType.PostJobListing)))
            .ForMember(dest => dest.CanReplyToReviews, opt => opt.MapFrom(src => src.Features.Contains(FeatureType.RespondToReviews)));
    }
}

/// <summary>
/// Şirket üyelik DTO'su
/// </summary>
public record CompanySubscriptionDto
{
    public string Id { get; init; } = string.Empty;
    public string CompanyId { get; init; } = string.Empty;
    public SubscriptionType Type { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public DateTime NextBillingDate { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int MaxJobPostings { get; init; }
    public int ActiveJobPostings { get; init; }
    public int RemainingJobPostings { get; init; }
    public int MaxHRPersonnel { get; init; }
    public int RemainingHRPersonnel { get; init; }
    public bool CanPostJobs { get; init; }
    public bool CanReplyToReviews { get; init; }
    public List<string> AuthorizedPersonnelIds { get; init; } = new();
}
