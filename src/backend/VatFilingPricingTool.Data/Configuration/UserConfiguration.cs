using Microsoft.EntityFrameworkCore; // Microsoft.EntityFrameworkCore package version 6.0.0
using Microsoft.EntityFrameworkCore.Metadata.Builders; // Microsoft.EntityFrameworkCore package version 6.0.0
using VatFilingPricingTool.Domain.Entities.User;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Data.Configuration
{
    /// <summary>
    /// Configuration class for the User entity in Entity Framework Core
    /// </summary>
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        /// <summary>
        /// Configures the User entity for Entity Framework Core
        /// </summary>
        /// <param name="builder">The entity type builder</param>
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Configure the table name as 'Users'
            builder.ToTable("Users");
            
            // Configure UserId as the primary key with maximum length of 36
            builder.HasKey(u => u.UserId);
            builder.Property(u => u.UserId).HasMaxLength(36).IsRequired();
            
            // Configure Email as a required string with maximum length of 256 and create a unique index
            builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
            builder.HasIndex(u => u.Email).IsUnique();
            
            // Configure FirstName as a required string with maximum length of 100
            builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            
            // Configure LastName as a required string with maximum length of 100
            builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            
            // Configure Role as a required enum with conversion to string
            builder.Property(u => u.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
            
            // Configure CreatedDate as a required datetime with default value of DateTime.UtcNow
            builder.Property(u => u.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            
            // Configure LastLoginDate as a required datetime with default value of DateTime.UtcNow
            builder.Property(u => u.LastLoginDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            
            // Configure IsActive as a required boolean with default value of true
            builder.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            // Configure AzureAdObjectId as an optional string with maximum length of 36
            builder.Property(u => u.AzureAdObjectId)
                .HasMaxLength(36)
                .IsRequired(false);
            
            // Configure one-to-many relationship with Calculation entity
            builder.HasMany(u => u.Calculations)
                .WithOne()
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Restrict);
            
            // Configure one-to-many relationship with Report entity
            builder.HasMany(u => u.Reports)
                .WithOne()
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Restrict);
            
            // Configure one-to-many relationship with Integration entity
            builder.HasMany(u => u.Integrations)
                .WithOne()
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Restrict);
            
            // Create an index on IsActive for faster filtering of active users
            builder.HasIndex(u => u.IsActive);
            
            // Create an index on Role for faster filtering by user role
            builder.HasIndex(u => u.Role);
        }
    }
}