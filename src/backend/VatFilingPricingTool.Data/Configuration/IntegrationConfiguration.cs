using Microsoft.EntityFrameworkCore; // Microsoft.EntityFrameworkCore package version 6.0.0
using Microsoft.EntityFrameworkCore.Metadata.Builders; // Microsoft.EntityFrameworkCore package version 6.0.0
using System.Text.Json; // System.Text.Json package version 6.0.0
using VatFilingPricingTool.Domain.Entities;

namespace VatFilingPricingTool.Data.Configuration
{
    /// <summary>
    /// Configuration class for the Integration entity in Entity Framework Core.
    /// Defines how the Integration entity is mapped to the database schema.
    /// </summary>
    public class IntegrationConfiguration : IEntityTypeConfiguration<Integration>
    {
        /// <summary>
        /// Configures the Integration entity for Entity Framework Core.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<Integration> builder)
        {
            // Define JSON serialization options for the AdditionalSettings dictionary
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNameCaseInsensitive = true
            };

            // Configure the table name as 'Integrations'
            builder.ToTable("Integrations");

            // Configure IntegrationId as the primary key with maximum length of 36
            builder.HasKey(e => e.IntegrationId);
            builder.Property(e => e.IntegrationId)
                .HasMaxLength(36)
                .IsRequired();

            // Configure UserId as a required string with maximum length of 36
            builder.Property(e => e.UserId)
                .HasMaxLength(36)
                .IsRequired();

            // Configure SystemType as a required string with maximum length of 50
            builder.Property(e => e.SystemType)
                .HasMaxLength(50)
                .IsRequired()
                .HasComment("Type of the external system (e.g., Dynamics365, CognitiveServices)");

            // Configure ConnectionString as a required string with maximum length of 500
            builder.Property(e => e.ConnectionString)
                .HasMaxLength(500)
                .IsRequired()
                .HasComment("Connection string to the external system (encrypted when stored)");

            // Configure LastSyncDate as a required datetime with default value of UTC now
            builder.Property(e => e.LastSyncDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("Date and time of the last successful synchronization");

            // Configure IsActive as a required boolean with default value of false
            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Indicates if this integration is currently active");

            // Configure RetryCount as a required integer with default value of 0
            builder.Property(e => e.RetryCount)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Number of connection retry attempts since last successful connection");

            // Configure ApiKey as an optional string with maximum length of 500
            builder.Property(e => e.ApiKey)
                .HasMaxLength(500)
                .IsRequired(false)
                .HasComment("API key used for authentication with the external system");

            // Configure ApiEndpoint as an optional string with maximum length of 500
            builder.Property(e => e.ApiEndpoint)
                .HasMaxLength(500)
                .IsRequired(false)
                .HasComment("API endpoint URL for the external system");

            // Configure AdditionalSettings as a JSON-serialized string with conversion to Dictionary<string, string>
            builder.Property(e => e.AdditionalSettings)
                .HasConversion(
                    v => v != null ? JsonSerializer.Serialize(v, jsonOptions) : null,
                    v => string.IsNullOrEmpty(v) ? new Dictionary<string, string>() : JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonOptions))
                .HasColumnType("nvarchar(max)")
                .HasComment("Additional configuration settings stored as JSON");

            // Configure one-to-many relationship with User entity
            builder.HasOne(e => e.User)
                .WithMany(u => u.Integrations)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create an index on UserId for faster lookup of user integrations
            builder.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_Integrations_UserId");

            // Create an index on SystemType for faster filtering by integration type
            builder.HasIndex(e => e.SystemType)
                .HasDatabaseName("IX_Integrations_SystemType");

            // Create an index on IsActive for faster filtering of active integrations
            builder.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_Integrations_IsActive");
        }
    }
}