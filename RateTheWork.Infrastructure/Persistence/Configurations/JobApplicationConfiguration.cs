using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RateTheWork.Domain.Entities;
using System.Text.Json;

namespace RateTheWork.Infrastructure.Persistence.Configurations;

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.ToTable("JobApplications");
        
        builder.HasKey(j => j.Id);
        
        // Configure JSON columns
        builder.Property(j => j.SkillAssessments)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, (JsonSerializerOptions)null) ?? new Dictionary<string, int>())
            .HasColumnType("jsonb");
            
        // Configure StatusHistory as JSON
        builder.Property(j => j.StatusHistory)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<ApplicationStatusHistory>>(v, (JsonSerializerOptions)null) ?? new List<ApplicationStatusHistory>())
            .HasColumnType("jsonb");
    }
}