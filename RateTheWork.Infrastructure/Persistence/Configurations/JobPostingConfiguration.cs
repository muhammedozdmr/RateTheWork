using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RateTheWork.Domain.Entities;
using System.Text.Json;

namespace RateTheWork.Infrastructure.Persistence.Configurations;

public class JobPostingConfiguration : IEntityTypeConfiguration<JobPosting>
{
    public void Configure(EntityTypeBuilder<JobPosting> builder)
    {
        builder.ToTable("JobPostings");
        
        builder.HasKey(j => j.Id);
        
        // Configure JSON columns
        builder.Property(j => j.AdditionalInfo)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null) ?? new Dictionary<string, string>())
            .HasColumnType("jsonb");
            
        builder.Property(j => j.Skills)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>())
            .HasColumnType("jsonb");
            
        builder.Property(j => j.Benefits)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>())
            .HasColumnType("jsonb");
    }
}