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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}