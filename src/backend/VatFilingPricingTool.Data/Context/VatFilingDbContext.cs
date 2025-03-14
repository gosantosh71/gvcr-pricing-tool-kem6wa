using Microsoft.EntityFrameworkCore; // Microsoft.EntityFrameworkCore package version 6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging package version 6.0.0
using System.Threading; // System.Threading.Tasks package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using VatFilingPricingTool.Data.Configuration; // Import for entity configurations
using VatFilingPricingTool.Data.Context; // Import for the interface
using VatFilingPricingTool.Data.Extensions; // Import for extension methods
using VatFilingPricingTool.Domain.Entities; // Import for entity classes

namespace VatFilingPricingTool.Data.Context
{
    /// <summary>
    /// Entity Framework Core DbContext implementation for the VAT Filing Pricing Tool
    /// </summary>
    public class VatFilingDbContext : DbContext, IVatFilingDbContext
    {
        /// <summary>
        /// Gets or sets the DbSet for User entities
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for Country entities
        /// </summary>
        public DbSet<Country> Countries { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for Service entities
        /// </summary>
        public DbSet<Service> Services { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for AdditionalService entities
        /// </summary>
        public DbSet<AdditionalService> AdditionalServices { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for Calculation entities
        /// </summary>
        public DbSet<Calculation> Calculations { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for CalculationCountry entities
        /// </summary>
        public DbSet<CalculationCountry> CalculationCountries { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for Rule entities
        /// </summary>
        public DbSet<Rule> Rules { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for RuleParameter entities
        /// </summary>
        public DbSet<RuleParameter> RuleParameters { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for Report entities
        /// </summary>
        public DbSet<Report> Reports { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for Integration entities
        /// </summary>
        public DbSet<Integration> Integrations { get; set; }

        private readonly ILogger<VatFilingDbContext> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="VatFilingDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        /// <param name="logger">The logger instance for logging database context operations.</param>
        public VatFilingDbContext(DbContextOptions<VatFilingDbContext> options, ILogger<VatFilingDbContext> logger)
            : base(options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Configures the model that was discovered by convention from the entity types
        /// exposed in <see cref="DbSet"/> properties on your derived context. The resulting model
        /// may be cached and re-used for later instances of your derived context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations
            modelBuilder.ApplyConfiguration(new CalculationConfiguration());
            modelBuilder.ApplyConfiguration(new CountryConfiguration());
            modelBuilder.ApplyConfiguration(new ServiceConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new ReportConfiguration());
            modelBuilder.ApplyConfiguration(new RuleConfiguration());
            modelBuilder.ApplyConfiguration(new IntegrationConfiguration());

            // Apply global conventions
            modelBuilder.ApplyGlobalConventions();

            // Seed initial data
            modelBuilder.SeedInitialData();

            _logger.LogInformation("Model creation completed.");
        }

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <returns>The number of state entries written to the database.</returns>
        public override int SaveChanges()
        {
            _logger.LogInformation("SaveChanges operation started.");
            var result = base.SaveChanges();
            _logger.LogInformation($"SaveChanges operation completed. {result} records affected.");
            return result;
        }

        /// <summary>
        /// Asynchronously saves all changes made in this context to the database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("SaveChangesAsync operation started.");
            var result = await base.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"SaveChangesAsync operation completed. {result} records affected.");
            return result;
        }

        /// <summary>
        /// Creates a <see cref="DbSet{TEntity}"/> that can be used to query and save instances of <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity for which a set should be returned.</typeparam>
        /// <returns>A set for the given entity type.</returns>
        public new DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();
        }
    }
}