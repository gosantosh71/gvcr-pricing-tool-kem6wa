using Microsoft.EntityFrameworkCore; // v6.0.0
using Microsoft.EntityFrameworkCore.Metadata.Builders; // v6.0.0
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.ValueObjects;

namespace VatFilingPricingTool.Data.Configuration
{
    /// <summary>
    /// Configuration class for the Service entity in Entity Framework Core.
    /// Defines how the Service entity is mapped to the database schema.
    /// </summary>
    public class ServiceConfiguration : IEntityTypeConfiguration<Service>
    {
        /// <summary>
        /// Configures the Service entity for Entity Framework Core.
        /// </summary>
        /// <param name="builder">The entity type builder used to configure the entity.</param>
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            // Configure the table name
            builder.ToTable("Services");

            // Configure the primary key
            builder.HasKey(s => s.ServiceId);
            builder.Property(s => s.ServiceId)
                .HasMaxLength(36)
                .IsRequired();

            // Configure basic properties
            builder.Property(s => s.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(s => s.Description)
                .HasMaxLength(500)
                .IsRequired();

            // Configure ComplexityLevel with range validation
            builder.Property(s => s.ComplexityLevel)
                .IsRequired()
                .HasAnnotation("Range", new[] { 1, 10 });

            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Configure the Money value object
            builder.OwnsOne(s => s.BasePrice, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("BasePriceAmount")
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("BasePriceCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            // Configure ServiceType enum to be stored as a string
            builder.Property(s => s.ServiceType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            // Configure one-to-many relationship with Calculation entity
            builder.HasMany(s => s.Calculations)
                .WithOne() // No explicit navigation property in Calculation pointing back to Service
                .HasForeignKey("ServiceId") // Foreign key property name in Calculation
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // Configure indexes for common query patterns
            builder.HasIndex(s => s.IsActive)
                .HasDatabaseName("IX_Services_IsActive");

            builder.HasIndex(s => s.ServiceType)
                .HasDatabaseName("IX_Services_ServiceType");

            builder.HasIndex(s => s.ComplexityLevel)
                .HasDatabaseName("IX_Services_ComplexityLevel");
        }
    }
}