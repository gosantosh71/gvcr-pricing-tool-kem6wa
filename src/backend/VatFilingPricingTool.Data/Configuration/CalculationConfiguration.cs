using Microsoft.EntityFrameworkCore; // v6.0.0
using Microsoft.EntityFrameworkCore.Metadata.Builders; // v6.0.0
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.ValueObjects;

namespace VatFilingPricingTool.Data.Configuration
{
    /// <summary>
    /// Configuration class for the Calculation entity in Entity Framework Core
    /// </summary>
    public class CalculationConfiguration : IEntityTypeConfiguration<Calculation>
    {
        /// <summary>
        /// Configures the Calculation entity for Entity Framework Core
        /// </summary>
        /// <param name="builder">The entity type builder</param>
        public void Configure(EntityTypeBuilder<Calculation> builder)
        {
            // Configure the table name
            builder.ToTable("Calculations");

            // Configure the primary key
            builder.HasKey(c => c.CalculationId);

            // Configure properties
            builder.Property(c => c.CalculationId)
                .IsRequired()
                .HasMaxLength(36); // Guid as string

            builder.Property(c => c.UserId)
                .IsRequired();

            builder.Property(c => c.ServiceId)
                .IsRequired();

            builder.Property(c => c.TransactionVolume)
                .IsRequired()
                .HasAnnotation("MinValue", 1); // Minimum value of 1

            // Configure enum conversion for FilingFrequency
            builder.Property(c => c.FilingFrequency)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            // Configure Money value object conversion for TotalCost
            builder.OwnsOne(c => c.TotalCost, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("TotalCostAmount")
                    .IsRequired()
                    .HasPrecision(18, 2);

                money.Property(m => m.Currency)
                    .HasColumnName("TotalCostCurrency")
                    .IsRequired()
                    .HasMaxLength(3); // ISO 4217 currency code length
            });

            builder.Property(c => c.CalculationDate)
                .IsRequired();

            builder.Property(c => c.CurrencyCode)
                .IsRequired()
                .HasMaxLength(3); // ISO 4217 currency code length

            builder.Property(c => c.IsArchived)
                .IsRequired()
                .HasDefaultValue(false);

            // Configure relationships
            // One-to-many relationship with CalculationCountry
            builder.HasMany(c => c.CalculationCountries)
                .WithOne()
                .HasForeignKey("CalculationId")
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-many relationship with CalculationAdditionalService
            builder.HasMany(c => c.AdditionalServices)
                .WithOne()
                .HasForeignKey("CalculationId")
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-many relationship with Report
            builder.HasMany(c => c.Reports)
                .WithOne()
                .HasForeignKey("CalculationId")
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-one relationship with User
            builder.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-one relationship with Service
            builder.HasOne(c => c.Service)
                .WithMany()
                .HasForeignKey(c => c.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure indexes for performance
            builder.HasIndex(c => c.UserId)
                .HasDatabaseName("IX_Calculations_UserId");
            
            builder.HasIndex(c => c.CalculationDate)
                .HasDatabaseName("IX_Calculations_CalculationDate");
            
            builder.HasIndex(c => c.IsArchived)
                .HasDatabaseName("IX_Calculations_IsArchived");
        }
    }
}