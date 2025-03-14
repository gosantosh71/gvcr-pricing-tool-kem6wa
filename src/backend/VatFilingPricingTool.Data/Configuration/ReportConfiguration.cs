using Microsoft.EntityFrameworkCore; // Version: 6.0.0
using Microsoft.EntityFrameworkCore.Metadata.Builders; // Version: 6.0.0
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Data.Configuration
{
    /// <summary>
    /// Configuration class for the Report entity in Entity Framework Core
    /// </summary>
    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        /// <summary>
        /// Configures the Report entity for Entity Framework Core
        /// </summary>
        /// <param name="builder">The entity type builder for the Report entity</param>
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            // Configure the table name
            builder.ToTable("Reports");
            
            // Configure the primary key
            builder.HasKey(r => r.ReportId);
            
            // Configure properties
            builder.Property(r => r.ReportId)
                .IsRequired();
                
            builder.Property(r => r.UserId)
                .IsRequired();
                
            builder.Property(r => r.CalculationId)
                .IsRequired();
                
            builder.Property(r => r.ReportTitle)
                .IsRequired()
                .HasMaxLength(200);
                
            builder.Property(r => r.ReportType)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(r => r.Format)
                .IsRequired()
                .HasConversion<string>();
                
            builder.Property(r => r.StorageUrl)
                .HasMaxLength(1000);
                
            builder.Property(r => r.GenerationDate)
                .IsRequired()
                .HasDefaultValue(DateTime.UtcNow);
                
            builder.Property(r => r.FileSize)
                .IsRequired()
                .HasDefaultValue(0);
                
            builder.Property(r => r.IsArchived)
                .IsRequired()
                .HasDefaultValue(false);
                
            // Configure relationships
            builder.HasOne(r => r.User)
                .WithMany() // Assuming User entity has a collection of reports
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(r => r.Calculation)
                .WithMany() // Assuming Calculation entity has a collection of reports
                .HasForeignKey(r => r.CalculationId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Configure indexes for better query performance
            builder.HasIndex(r => r.UserId)
                .HasDatabaseName("IX_Reports_UserId");
                
            builder.HasIndex(r => r.CalculationId)
                .HasDatabaseName("IX_Reports_CalculationId");
                
            builder.HasIndex(r => r.GenerationDate)
                .HasDatabaseName("IX_Reports_GenerationDate");
                
            builder.HasIndex(r => r.IsArchived)
                .HasDatabaseName("IX_Reports_IsArchived");
        }
    }
}