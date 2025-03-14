using Microsoft.AspNetCore.Hosting; // Microsoft.AspNetCore.Hosting, Version=6.0.0
using Microsoft.AspNetCore.Mvc.Testing; // Microsoft.AspNetCore.Mvc.Testing, Version=6.0.0
using Microsoft.EntityFrameworkCore; // Microsoft.EntityFrameworkCore, Version=6.0.0
using Microsoft.EntityFrameworkCore.InMemory; // Microsoft.EntityFrameworkCore.InMemory, Version=6.0.0
using Microsoft.Extensions.DependencyInjection; // Microsoft.Extensions.DependencyInjection, Version=6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging, Version=6.0.0
using System; // System, Version=4.0.0.0
using System.Net.Http; // System.Net.Http, Version=4.0.0.0
using VatFilingPricingTool.Api; // Reference to the API project
using VatFilingPricingTool.Data.Context; // Import for VatFilingDbContext
using VatFilingPricingTool.IntegrationTests.Utilities; // Import for TestAuthHandler

namespace VatFilingPricingTool.IntegrationTests.TestServer
{
    /// <summary>
    /// Custom WebApplicationFactory that configures a test server with in-memory services for integration testing
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private ILogger<CustomWebApplicationFactory> _logger;
        public string DatabaseName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the CustomWebApplicationFactory class
        /// </summary>
        public CustomWebApplicationFactory()
        {
            // Generate a unique database name using Guid.NewGuid()
            DatabaseName = Guid.NewGuid().ToString();

            // Create an ILoggerFactory instance
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });

            // Create a logger for the factory
            _logger = loggerFactory.CreateLogger<CustomWebApplicationFactory>();
        }

        /// <summary>
        /// Configures the web host with test services
        /// </summary>
        /// <param name="builder">The web host builder</param>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            // Configure builder to use test environment
            builder.UseEnvironment("Testing");

            // Configure services to replace real services with test versions
            builder.ConfigureServices(services =>
            {
                // Configure in-memory database
                ConfigureInMemoryDatabase(services);

                // Configure test authentication
                ConfigureTestAuthentication(services);

                // Add test logging
                services.AddSingleton<ILogger<CustomWebApplicationFactory>>(_logger);
            });
        }

        /// <summary>
        /// Creates a default HTTP client for making requests to the test server
        /// </summary>
        /// <returns>An HTTP client configured for the test server</returns>
        public override HttpClient CreateDefaultClient()
        {
            // Create a client using base.CreateDefaultClient()
            var client = base.CreateDefaultClient();

            // Configure default headers
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            // Return the configured client
            return client;
        }

        /// <summary>
        /// Creates an HTTP client with authentication for a specific user role
        /// </summary>
        /// <param name="userRole">The user role to authenticate with</param>
        /// <returns>An authenticated HTTP client</returns>
        public HttpClient CreateClient(string userRole)
        {
            // Create a client using base.CreateDefaultClient()
            var client = base.CreateDefaultClient();

            // Add Authorization header with the specified role
            client.DefaultRequestHeaders.Add("Authorization", userRole);

            // Configure default headers
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            // Return the configured client
            return client;
        }

        /// <summary>
        /// Configures an in-memory database for testing
        /// </summary>
        /// <param name="services">The service collection</param>
        private void ConfigureInMemoryDatabase(IServiceCollection services)
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbContextOptions<VatFilingDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database context with unique database name
            services.AddDbContext<VatFilingDbContext>(options =>
            {
                options.UseInMemoryDatabase(DatabaseName);
            });

            // Build the service provider to resolve the DbContext
            var sp = services.BuildServiceProvider();

            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<VatFilingDbContext>();
                var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

                // Ensure database is created
                db.Database.EnsureCreated();

                try
                {
                    // Seed database with test data
                    TestDatabaseInitializer.InitializeDbForTests(db);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred seeding the " +
                        $"database with test messages. Error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Configures test authentication for integration tests
        /// </summary>
        /// <param name="services">The service collection</param>
        private void ConfigureTestAuthentication(IServiceCollection services)
        {
            // Remove existing authentication handlers
            services.AddTestAuthentication();
        }
    }
}