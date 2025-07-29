using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RateTheWork.Domain.Entities;

namespace RateTheWork.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);
            
        builder.HasIndex(u => u.Email)
            .IsUnique();
            
        builder.Property(u => u.AnonymousUsername)
            .IsRequired()
            .HasMaxLength(256);
            
        builder.HasIndex(u => u.AnonymousUsername)
            .IsUnique();
            
        builder.Property(u => u.EncryptedFirstName)
            .IsRequired()
            .HasMaxLength(256);
            
        builder.Property(u => u.EncryptedLastName)
            .IsRequired()
            .HasMaxLength(256);
            
        builder.Property(u => u.EncryptedTcIdentityNumber)
            .HasMaxLength(256);
            
        builder.HasIndex(u => u.EncryptedTcIdentityNumber)
            .IsUnique()
            .HasFilter("\"EncryptedTcIdentityNumber\" IS NOT NULL AND \"EncryptedTcIdentityNumber\" != ''");
            
        builder.Property(u => u.EncryptedPhoneNumber)
            .HasMaxLength(256);
            
        builder.Property(u => u.HashedPassword)
            .IsRequired();
            
        builder.Property(u => u.Profession)
            .HasMaxLength(100);
            
        builder.Property(u => u.EncryptedAddress)
            .HasMaxLength(500);
            
        builder.Property(u => u.EncryptedCity)
            .HasMaxLength(100);
            
        builder.Property(u => u.EncryptedDistrict)
            .HasMaxLength(100);
            
        builder.Property(u => u.EncryptedBirthDate)
            .HasMaxLength(100);
            
        // Enum property
        builder.Property(u => u.Gender)
            .HasConversion<string>()
            .HasMaxLength(50);
    }
}