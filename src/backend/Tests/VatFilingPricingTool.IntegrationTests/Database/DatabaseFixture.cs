using Microsoft.EntityFrameworkCore; // Microsoft.EntityFrameworkCore package version 6.0.0
using Microsoft.EntityFrameworkCore.InMemory; // Microsoft.EntityFrameworkCore.InMemory package version 6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging package version 6.0.0
using Microsoft.Extensions.Logging.Debug; // Microsoft.Extensions.Logging.Debug package version 6.0.0
using System; // System package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using Xunit; // xunit package version 2.4.1
using VatFilingPricingTool.Data.Context; // Import for VatFilingDbContext
using VatFilingPricingTool.Data.Context; // Import for the interface
using VatFilingPricingTool.IntegrationTests.Utilities; // Import for IntegrationTestHelpers

namespace VatFilingPricingTool.IntegrationTests.Database
{
    /// <summary>
    /// Test fixture that provides a shared database context for integration tests
    /// </summary>
    public class DatabaseFixture : IDisposable
    {
        /// <summary>
        /// Gets the database context for the test fixture
        /// </summary>
        public IVatFilingDbContext DbContext { get; private set; }

        /// <summary>
        /// Gets the unique name of the in-memory database
        /// </summary>
        public string DatabaseName { get; private set; }

        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of the DatabaseFixture class
        /// </summary>
        public DatabaseFixture()
        {
            // Generate a unique database name using Guid.NewGuid()
            DatabaseName = Guid.NewGuid().ToString();

            // Create an ILoggerFactory instance for logging
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddDebug();
            });

            // Configure DbContextOptions with the in-memory database provider
            var options = CreateDbContextOptions();

            // Create a new VatFilingDbContext with the options and logger
            DbContext = new VatFilingDbContext(options, _loggerFactory.CreateLogger<VatFilingDbContext>());

            // Ensure the database is created
            ((DbContext)DbContext).Database.EnsureCreated();

            // Seed the database with initial test data
            SeedDatabase().Wait();
        }

        /// <summary>
        /// Resets the database to a clean state for a new test
        /// </summary>
        public async Task ResetDatabase()
        {
            // Delete all existing data from all tables
            await ((DbContext)DbContext).Database.EnsureDeletedAsync();
            await ((DbContext)DbContext).Database.EnsureCreatedAsync();

            // Seed the database with fresh test data
            await SeedDatabase();
        }

        /// <summary>
        /// Seeds the database with initial test data
        /// </summary>
        private async Task SeedDatabase()
        {
            // Generate test countries using TestDataGenerator
            var countries = IntegrationTestHelpers.TestDataGenerator.GenerateCountries(3);

            // Add countries to the database
            ((DbContext)DbContext).Set<Domain.Entities.Country>().AddRange(countries);

            // Generate test services
            var services = new List<Domain.Entities.Service>
            {
                Domain.Entities.Service.Create("Standard Filing", "Basic VAT filing service", 800, "EUR", Domain.Enums.ServiceType.StandardFiling, 1),
                Domain.Entities.Service.Create("Complex Filing", "Enhanced VAT filing service", 1200, "EUR", Domain.Enums.ServiceType.ComplexFiling, 3),
                Domain.Entities.Service.Create("Priority Service", "Premium VAT filing service", 1500, "EUR", Domain.Enums.ServiceType.PriorityService, 5)
            };

            // Add services to the database
            ((DbContext)DbContext).Set<Domain.Entities.Service>().AddRange(services);

            // Generate test rules for each country
            var rules = new List<Domain.Entities.Rule>();
            foreach (var country in countries)
            {
                rules.AddRange(IntegrationTestHelpers.TestDataGenerator.GenerateRules(country.CountryCode));
            }

            // Add rules to the database
            ((DbContext)DbContext).Set<Domain.Entities.Rule>().AddRange(rules);

            // Generate test users with different roles
            var users = IntegrationTestHelpers.TestDataGenerator.GenerateUsers();

            // Add users to the database
            ((DbContext)DbContext).Set<Domain.Entities.User>().AddRange(users);

            // Save all changes to the database
            await DbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Disposes of resources used by the fixture
        /// </summary>
        public void Dispose()
        {
            // Dispose of the database context
            DbContext?.Dispose();

            // Dispose of the logger factory
            _loggerFactory?.Dispose();
        }

        /// <summary>
        /// Creates DbContextOptions for the in-memory database
        /// </summary>
        private DbContextOptions<VatFilingDbContext> CreateDbContextOptions()
        {
            // Create a new DbContextOptionsBuilder
            var builder = new DbContextOptionsBuilder<VatFilingDbContext>();

            // Configure to use the in-memory database provider with the unique database name
            builder.UseInMemoryDatabase(DatabaseName);

            // Configure logging options
            builder.UseLoggerFactory(_loggerFactory);

            // Return the built options
            return builder.Options;
        }
    }
}