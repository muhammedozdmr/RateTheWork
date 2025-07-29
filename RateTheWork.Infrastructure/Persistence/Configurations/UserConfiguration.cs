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
            
        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(256);
            
        builder.HasIndex(u => u.UserName)
            .IsUnique();
            
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(u => u.TCIdentityNumber)
            .HasMaxLength(11)
            .IsFixedLength();
            
        builder.HasIndex(u => u.TCIdentityNumber)
            .IsUnique()
            .HasFilter("\"TCIdentityNumber\" IS NOT NULL");
            
        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);
            
        builder.Property(u => u.PasswordHash)
            .IsRequired();
            
        builder.Property(u => u.SecurityStamp)
            .IsRequired();
            
        builder.Property(u => u.ProfilePictureUrl)
            .HasMaxLength(500);
            
        builder.Property(u => u.Bio)
            .HasMaxLength(1000);
            
        builder.HasOne(u => u.Subscription)
            .WithOne(s => s.User)
            .HasForeignKey<Subscription>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(u => u.Reviews)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(u => u.Warnings)
            .WithOne(w => w.User)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(u => u.Bans)
            .WithOne(b => b.User)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}