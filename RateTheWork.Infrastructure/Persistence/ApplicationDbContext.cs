using Microsoft.EntityFrameworkCore;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Common;
using System.Reflection;

namespace RateTheWork.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewVote> ReviewVotes => Set<ReviewVote>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<Warning> Warnings => Set<Warning>();
    public DbSet<Ban> Bans => Set<Ban>();
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<CVApplication> CVApplications => Set<CVApplication>();
    public DbSet<ContractorReview> ContractorReviews => Set<ContractorReview>();
    public DbSet<JobAlert> JobAlerts => Set<JobAlert>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<HRPersonnel> HRPersonnel => Set<HRPersonnel>();
    public DbSet<CompanyBranch> CompanyBranches => Set<CompanyBranch>();
    public DbSet<CompanySubscription> CompanySubscriptions => Set<CompanySubscription>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UserBadge> UserBadges => Set<UserBadge>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Ignore value objects that are not entities
        // Company value objects
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Company.CompanyReviewStatistics>();
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Company.CompanyGrowthAnalysis>();
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Company.CompanyRiskScore>();
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Company.CompanyCategory>();
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Company.TaxId>();
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Company.MersisNo>();
        
        // CV value objects
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.CV.CVAnalysisResult>();
        
        // Moderation value objects
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Moderation.ModerationResult>();
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Moderation.ModerationDetails>();
        
        // Review value objects
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Review.ReviewQualityScore>();
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Review.SentimentAnalysisResult>();
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Review.ReviewTrends>();
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Review.ContentCategory>();
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Review.VoteStatus>();
        
        // User value objects
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.User.UserActivitySummary>();
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.User.UserBehaviorScore>();
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.User.UserPreferences>();
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.User.BadgeProgress>();
        
        // Subscription value objects
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Subscription.SubscriptionPlan>();
        
        // Common value objects
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Common.Address>();
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Common.Coordinate>();
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Common.DateRange>();
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Common.Email>();
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Common.Money>();
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Common.PhoneNumber>();
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Common.Rating>();
        modelBuilder.Ignore<RateTheWork.Domain.ValueObjects.Common.ValueObject>();
        
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // BaseEntity's CreatedAt is set in constructor, ModifiedAt is managed by domain logic
        // No need to override timestamps here
        return base.SaveChangesAsync(cancellationToken);
    }
}