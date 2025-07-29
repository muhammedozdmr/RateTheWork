using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.Subscription;
using System.Text.Json;

namespace RateTheWork.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");
        
        builder.HasKey(s => s.Id);
        
        // Configure properties
        builder.Property(s => s.UserId)
            .IsRequired()
            .HasMaxLength(450);
            
        builder.Property(s => s.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("TRY");
            
        builder.Property(s => s.CancellationReason)
            .HasMaxLength(500);
            
        // Configure JSON columns for dictionaries
        builder.Property(s => s.Features)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<FeatureType>>(v, (JsonSerializerOptions)null) ?? new List<FeatureType>())
            .HasColumnType("jsonb");
            
        builder.Property(s => s.UsageLimits)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, (JsonSerializerOptions)null) ?? new Dictionary<string, int>())
            .HasColumnType("jsonb");
            
        builder.Property(s => s.CurrentUsage)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, (JsonSerializerOptions)null) ?? new Dictionary<string, int>())
            .HasColumnType("jsonb");
    }
}