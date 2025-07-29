using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RateTheWork.Domain.Entities;
using System.Text.Json;

namespace RateTheWork.Infrastructure.Persistence.Configurations;

public class CVApplicationConfiguration : IEntityTypeConfiguration<CVApplication>
{
    public void Configure(EntityTypeBuilder<CVApplication> builder)
    {
        builder.ToTable("CVApplications");
        
        builder.HasKey(c => c.Id);
        
        // Configure JSON columns
        builder.Property(c => c.AdditionalInfo)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null) ?? new Dictionary<string, string>())
            .HasColumnType("jsonb");
    }
}