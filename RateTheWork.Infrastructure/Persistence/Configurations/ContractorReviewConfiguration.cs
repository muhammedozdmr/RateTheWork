using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RateTheWork.Domain.Entities;

namespace RateTheWork.Infrastructure.Persistence.Configurations;

public class ContractorReviewConfiguration : IEntityTypeConfiguration<ContractorReview>
{
    public void Configure(EntityTypeBuilder<ContractorReview> builder)
    {
        builder.ToTable("ContractorReviews");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProjectDescription)
            .HasMaxLength(2000);

        builder.Property(x => x.ProjectBudget)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.ProjectDuration)
            .HasMaxLength(100);

        builder.Property(x => x.PaymentTimelinessRating)
            .HasColumnType("decimal(3,2)");

        builder.Property(x => x.CommunicationRating)
            .HasColumnType("decimal(3,2)");

        builder.Property(x => x.ProjectManagementRating)
            .HasColumnType("decimal(3,2)");

        builder.Property(x => x.TechnicalCompetenceRating)
            .HasColumnType("decimal(3,2)");

        builder.Property(x => x.OverallRating)
            .HasColumnType("decimal(3,2)");

        builder.Property(x => x.ReviewText)
            .HasMaxLength(5000);

        builder.Property(x => x.WouldWorkAgain)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(50);

        builder.Property(x => x.VerificationDocumentUrl)
            .HasMaxLength(500);

        // Foreign key relationships
        builder.Property(x => x.CompanyId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ReviewedById)
            .IsRequired()
            .HasMaxLength(50);

        // Navigation properties
        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.CompanyId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ReviewedById);
        builder.HasIndex(x => x.IsVerified);
        builder.HasIndex(x => x.CreatedAt);
    }
}