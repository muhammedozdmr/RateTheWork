using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RateTheWork.Domain.Entities;
using System.Text.Json;

namespace RateTheWork.Infrastructure.Persistence.Configurations;

public class SystemReportConfiguration : IEntityTypeConfiguration<SystemReport>
{
    public void Configure(EntityTypeBuilder<SystemReport> builder)
    {
        builder.ToTable("SystemReports");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        // Parameters dictionary'yi JSON olarak sakla
        builder.Property(x => x.Parameters)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null!) 
                    ?? new Dictionary<string, object>())
            .HasColumnType("jsonb");

        builder.Property(x => x.FileUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedById);
        builder.HasIndex(x => x.GeneratedAt);
    }
}