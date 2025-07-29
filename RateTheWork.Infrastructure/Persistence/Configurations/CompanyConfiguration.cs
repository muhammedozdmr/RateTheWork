using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RateTheWork.Domain.Entities;
using System.Text.Json;

namespace RateTheWork.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        
        builder.HasKey(c => c.Id);
        
        // Configure string properties
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(c => c.TaxId)
            .IsRequired()
            .HasMaxLength(10);
            
        builder.Property(c => c.MersisNo)
            .IsRequired()
            .HasMaxLength(16);
            
        builder.Property(c => c.Description)
            .HasMaxLength(2000);
            
        builder.Property(c => c.Address)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(c => c.City)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(256);
            
        // Configure JSON columns
        builder.Property(c => c.Metadata)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null) ?? new Dictionary<string, object>())
            .HasColumnType("jsonb");
            
        builder.Property(c => c.Tags)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>())
            .HasColumnType("jsonb");
            
        builder.Property(c => c.Benefits)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>())
            .HasColumnType("jsonb");
            
        builder.Property(c => c.GalleryImages)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>())
            .HasColumnType("jsonb");
            
        builder.Property(c => c.WorkingHours)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null))
            .HasColumnType("jsonb");
            
        builder.Property(c => c.RatingBreakdown)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, decimal>>(v, (JsonSerializerOptions)null) ?? new Dictionary<string, decimal>())
            .HasColumnType("jsonb");
            
        builder.Property(c => c.ReviewCountByType)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, (JsonSerializerOptions)null) ?? new Dictionary<string, int>())
            .HasColumnType("jsonb");
            
        builder.Property(c => c.VerificationMetadata)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null))
            .HasColumnType("jsonb");
            
        builder.Property(c => c.ComplianceCertificates)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>())
            .HasColumnType("jsonb");
            
        builder.Property(c => c.SubsidiaryIds)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>())
            .HasColumnType("jsonb");
            
        // Ignore value object properties - they should be configured as owned entities or ignored
        builder.Ignore(c => c.ReviewStatistics);
        
        // Configure relationships
        builder.HasMany(c => c.Branches)
            .WithOne()
            .HasForeignKey("CompanyId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.Departments)
            .WithOne()
            .HasForeignKey("CompanyId")
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(c => c.CVApplications)
            .WithOne()
            .HasForeignKey("CompanyId")
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(c => c.ContractorReviews)
            .WithOne()
            .HasForeignKey("CompanyId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}