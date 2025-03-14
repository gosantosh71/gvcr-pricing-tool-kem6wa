using Microsoft.EntityFrameworkCore; // Version 6.0.0
using Microsoft.EntityFrameworkCore.Metadata.Builders; // Version 6.0.0
using System; // Version 6.0.0
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.ValueObjects;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Data.Configuration
{
    /// <summary>
    /// Configuration class for the Rule entity in Entity Framework Core
    /// </summary>
    public class RuleConfiguration : IEntityTypeConfiguration<Rule>
    {
        /// <summary>
        /// Configures the Rule entity for Entity Framework Core
        /// </summary>
        /// <param name="builder">Entity type builder for Rule</param>
        public void Configure(EntityTypeBuilder<Rule> builder)
        {
            // Configure the table name as 'Rules'
            builder.ToTable("Rules");
            
            // Configure RuleId as the primary key with type string
            builder.HasKey(r => r.RuleId);
            
            // Configure CountryCode as required with conversion from CountryCode value object to string
            builder.Property(r => r.CountryCode)
                .IsRequired()
                .HasConversion(
                    cc => cc.Value,
                    value => CountryCode.Create(value));
                
            // Configure Type as required with conversion from RuleType enum to string
            builder.Property(r => r.Type)
                .IsRequired()
                .HasConversion<string>();
                
            // Configure Name as required string with maximum length of 100
            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            // Configure Description as optional string with maximum length of 500
            builder.Property(r => r.Description)
                .HasMaxLength(500);
                
            // Configure Expression as required string with maximum length of 1000
            builder.Property(r => r.Expression)
                .IsRequired()
                .HasMaxLength(1000);
                
            // Configure EffectiveFrom as required DateTime
            builder.Property(r => r.EffectiveFrom)
                .IsRequired();
                
            // Configure EffectiveTo as optional DateTime
            builder.Property(r => r.EffectiveTo);
                
            // Configure Priority as required int with default value
            builder.Property(r => r.Priority)
                .IsRequired()
                .HasDefaultValue(100);
                
            // Configure IsActive as required boolean with default value of true
            builder.Property(r => r.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
                
            // Configure LastUpdated as required DateTime with default value of DateTime.UtcNow
            builder.Property(r => r.LastUpdated)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
                
            // Configure one-to-many relationship with RuleParameter entity
            builder.HasMany(r => r.Parameters)
                .WithOne(p => p.Rule)
                .HasForeignKey("RuleId")
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure owned entity relationship with RuleCondition collection
            builder.OwnsMany(r => r.Conditions, cb => 
            {
                cb.WithOwner().HasForeignKey("RuleId");
                cb.Property(c => c.Parameter).IsRequired().HasMaxLength(100);
                cb.Property(c => c.Operator).IsRequired().HasMaxLength(20);
                cb.Property(c => c.Value).IsRequired().HasMaxLength(100);
            });
            
            // Configure many-to-one relationship with Country entity
            builder.HasOne(r => r.Country)
                .WithMany()
                .HasForeignKey("CountryCode");
                
            // Create an index on CountryCode for faster filtering by country
            builder.HasIndex(r => r.CountryCode);
            
            // Create an index on IsActive for faster filtering of active rules
            builder.HasIndex(r => r.IsActive);
            
            // Create a composite index on CountryCode and Type for faster filtering by country and rule type
            builder.HasIndex(r => new { r.CountryCode, r.Type });
            
            // Create an index on EffectiveFrom and EffectiveTo for date range queries
            builder.HasIndex(r => new { r.EffectiveFrom, r.EffectiveTo });
        }
    }
}