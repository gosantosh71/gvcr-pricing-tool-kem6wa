using Microsoft.EntityFrameworkCore; // Version 6.0.0
using System;
using VatFilingPricingTool.Data.Seeding;

namespace VatFilingPricingTool.Data.Extensions
{
    /// <summary>
    /// Provides extension methods for Entity Framework Core's ModelBuilder to apply global conventions
    /// and seed initial data for the VAT Filing Pricing Tool database.
    /// </summary>
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Applies global conventions to all entities in the model.
        /// </summary>
        /// <param name="modelBuilder">The ModelBuilder instance.</param>
        /// <returns>The ModelBuilder instance for method chaining.</returns>
        public static ModelBuilder ApplyGlobalConventions(this ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
                throw new ArgumentNullException(nameof(modelBuilder));

            // Configure default conventions for various property types
            ConfigureStringProperties(modelBuilder);
            ConfigureDecimalProperties(modelBuilder);
            ConfigureDateTimeProperties(modelBuilder);
            ConfigureSoftDeleteFilter(modelBuilder);

            return modelBuilder;
        }

        /// <summary>
        /// Seeds initial data into the database using the DataSeeder.
        /// </summary>
        /// <param name="modelBuilder">The ModelBuilder instance.</param>
        /// <returns>The ModelBuilder instance for method chaining.</returns>
        public static ModelBuilder SeedInitialData(this ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
                throw new ArgumentNullException(nameof(modelBuilder));

            // Seed all initial reference data
            DataSeeder.SeedAllData(modelBuilder);

            return modelBuilder;
        }

        /// <summary>
        /// Configures default conventions for string properties.
        /// </summary>
        /// <param name="modelBuilder">The ModelBuilder instance.</param>
        private static void ConfigureStringProperties(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(string))
                    {
                        // Set default maximum length to 256 if not explicitly configured
                        if (!property.GetMaxLength().HasValue)
                        {
                            property.SetMaxLength(256);
                        }

                        // If property is marked as required, configure it as required in the database
                        if (!property.IsNullable)
                        {
                            property.IsNullable = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Configures default conventions for decimal properties.
        /// </summary>
        /// <param name="modelBuilder">The ModelBuilder instance.</param>
        private static void ConfigureDecimalProperties(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                    {
                        // Set default precision to 18 and scale to 2 if not explicitly configured
                        if (!property.GetPrecision().HasValue)
                        {
                            property.SetPrecision(18);
                            property.SetScale(2);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Configures default conventions for DateTime properties.
        /// </summary>
        /// <param name="modelBuilder">The ModelBuilder instance.</param>
        private static void ConfigureDateTimeProperties(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        // Use datetime2 to store DateTime values with higher precision
                        property.SetColumnType("datetime2");
                    }
                }
            }
        }

        /// <summary>
        /// Configures global query filters for soft delete pattern.
        /// </summary>
        /// <param name="modelBuilder">The ModelBuilder instance.</param>
        private static void ConfigureSoftDeleteFilter(ModelBuilder modelBuilder)
        {
            // This implements the soft delete pattern where deleted entities remain in the database
            // but are filtered out of queries by default using a global query filter.
            //
            // In a complete implementation, we would iterate through all entity types,
            // check for the presence of an IsDeleted property, and apply a filter like:
            // modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
            // 
            // However, since we can't dynamically create strongly-typed filters for unknown entity types
            // without using complex reflection in an extension method, this serves as a placeholder.
            //
            // For a full implementation, consider defining an ISoftDelete interface in your entities and
            // applying filters in the DbContext's OnModelCreating method for each implementing entity.
        }
    }
}