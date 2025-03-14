using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations; // Version 6.0.0
using VatFilingPricingTool.Data.Seeding;

namespace VatFilingPricingTool.Data.Migrations
{
    /// <summary>
    /// Static class responsible for seeding initial reference data during database migrations
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// Private constructor to prevent instantiation of static class
        /// </summary>
        private SeedData()
        {
            // Private constructor to enforce static usage
        }

        /// <summary>
        /// Seeds the database with initial reference data during migrations
        /// </summary>
        /// <param name="migrationBuilder">The migration builder to use for seeding</param>
        public static void SeedDatabase(MigrationBuilder migrationBuilder)
        {
            // Create a ModelBuilder instance using migrationBuilder.CreateBuilder()
            var modelBuilder = migrationBuilder.CreateBuilder();
            
            // Call DataSeeder.SeedAllData(modelBuilder) to seed all initial reference data
            DataSeeder.SeedAllData(modelBuilder);
            
            // Log completion of database seeding process
            System.Diagnostics.Debug.WriteLine("Database seeding completed successfully");
        }
    }
}