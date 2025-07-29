using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RateTheWork.Domain.Entities;
using System.Text.Json;

namespace RateTheWork.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        
        builder.HasKey(n => n.Id);
        
        // Configure JSON columns
        builder.Property(n => n.Data)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null))
            .HasColumnType("jsonb");
    }
}