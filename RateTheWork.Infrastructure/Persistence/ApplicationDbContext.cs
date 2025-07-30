using System.Reflection;
using Microsoft.EntityFrameworkCore;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.ValueObjects.Common;
using RateTheWork.Domain.ValueObjects.Company;
using RateTheWork.Domain.ValueObjects.CV;
using RateTheWork.Domain.ValueObjects.Moderation;
using RateTheWork.Domain.ValueObjects.Review;
using RateTheWork.Domain.ValueObjects.Subscription;
using RateTheWork.Domain.ValueObjects.User;

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
    public DbSet<SystemReport> SystemReports => Set<SystemReport>();
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
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<VerificationRequest> VerificationRequests => Set<VerificationRequest>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Entity olmayan değer nesnelerini yok say
        // Şirket değer nesneleri
        modelBuilder.Ignore<CompanyReviewStatistics>();
        modelBuilder.Ignore<CompanyGrowthAnalysis>();
        modelBuilder.Ignore<CompanyRiskScore>();
        modelBuilder.Ignore<CompanyCategory>();
        modelBuilder.Ignore<TaxId>();
        modelBuilder.Ignore<MersisNo>();

        // CV değer nesneleri
        modelBuilder.Ignore<CVAnalysisResult>();

        // Moderasyon değer nesneleri
        modelBuilder.Ignore<ModerationResult>();
        modelBuilder.Ignore<ModerationDetails>();

        // İnceleme değer nesneleri
        modelBuilder.Ignore<ReviewQualityScore>();
        modelBuilder.Ignore<SentimentAnalysisResult>();
        modelBuilder.Ignore<ReviewTrends>();
        modelBuilder.Ignore<ContentCategory>();
        modelBuilder.Ignore<VoteStatus>();

        // Kullanıcı değer nesneleri
        modelBuilder.Ignore<UserActivitySummary>();
        modelBuilder.Ignore<UserBehaviorScore>();
        modelBuilder.Ignore<UserPreferences>();
        modelBuilder.Ignore<BadgeProgress>();

        // Abonelik değer nesneleri
        modelBuilder.Ignore<SubscriptionPlan>();

        // Ortak değer nesneleri
        modelBuilder.Ignore<Address>();
        modelBuilder.Ignore<Coordinate>();
        modelBuilder.Ignore<DateRange>();
        modelBuilder.Ignore<Email>();
        modelBuilder.Ignore<Money>();
        modelBuilder.Ignore<PhoneNumber>();
        modelBuilder.Ignore<Rating>();
        modelBuilder.Ignore<ValueObject>();

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // BaseEntity'nin CreatedAt değeri yapıcıda ayarlanır, ModifiedAt alan adı mantığı tarafından yönetilir
        // Burada zaman damgalarını geçersiz kılmaya gerek yok
        return base.SaveChangesAsync(cancellationToken);
    }
}
