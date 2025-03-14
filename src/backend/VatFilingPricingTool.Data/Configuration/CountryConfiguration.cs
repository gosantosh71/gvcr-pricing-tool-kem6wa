using Microsoft.EntityFrameworkCore; // Version 6.0.0
using Microsoft.EntityFrameworkCore.Metadata.Builders; // Version 6.0.0
using System;
using System.Collections.Generic; // Version 6.0.0
using System.Linq;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.ValueObjects;

namespace VatFilingPricingTool.Data.Configuration
{
    /// <summary>
    /// Configuration class for the Country entity in Entity Framework Core
    /// </summary>
    public class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        /// <summary>
        /// Configures the Country entity for Entity Framework Core
        /// </summary>
        /// <param name="builder">Entity type builder for Country</param>
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            // Configure the table name as 'Countries'
            builder.ToTable("Countries");

            // Configure Code as the primary key with conversion from CountryCode to string
            builder.HasKey(c => c.Code);
            builder.Property(c => c.Code)
                .HasConversion(
                    code => code.Value,
                    value => CountryCode.Create(value))
                .HasMaxLength(2)
                .IsRequired();

            // Configure Name as a required string with maximum length of 100
            builder.Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();

            // Configure StandardVatRate as a required complex type with conversion to decimal and precision (5,2)
            builder.Property(c => c.StandardVatRate)
                .HasConversion(
                    rate => rate.Value,
                    value => VatRate.Create(value))
                .HasPrecision(5, 2)
                .IsRequired();

            // Configure CurrencyCode as a required string with maximum length of 3
            builder.Property(c => c.CurrencyCode)
                .HasMaxLength(3)
                .IsRequired();

            // Configure AvailableFilingFrequencies as a required collection with conversion to string
            builder.Property(c => c.AvailableFilingFrequencies)
                .HasConversion(
                    frequencies => string.Join(',', frequencies.Select(f => (int)f)),
                    value => value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(v => (FilingFrequency)int.Parse(v))
                             .ToHashSet())
                .IsRequired();

            // Configure IsActive as a required boolean with default value of true
            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Configure LastUpdated as a required datetime with default value of DateTime.UtcNow
            builder.Property(c => c.LastUpdated)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Configure one-to-many relationship with Rule entity
            builder.HasMany(c => c.Rules)
                .WithOne()
                .HasForeignKey("CountryCode")
                .OnDelete(DeleteBehavior.Cascade);

            // Configure many-to-many relationship with Calculation entity through CalculationCountry
            builder.HasMany(c => c.CalculationCountries)
                .WithOne()
                .HasForeignKey("CountryCode")
                .OnDelete(DeleteBehavior.Cascade);

            // Create an index on IsActive for faster filtering of active countries
            builder.HasIndex(c => c.IsActive);

            // Create an index on CurrencyCode for faster filtering by currency
            builder.HasIndex(c => c.CurrencyCode);
        }
    }
}