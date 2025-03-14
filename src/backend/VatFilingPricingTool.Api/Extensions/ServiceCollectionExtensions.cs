using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation.AspNetCore;
using VatFilingPricingTool.Service.Interfaces;
using VatFilingPricingTool.Service.Implementations;
using VatFilingPricingTool.Data.Context;
using VatFilingPricingTool.Data.Repositories;
using VatFilingPricingTool.Domain.Rules;
using VatFilingPricingTool.Infrastructure.Authentication;
using VatFilingPricingTool.Infrastructure.Caching;
using VatFilingPricingTool.Infrastructure.Integration;
using VatFilingPricingTool.Infrastructure.Storage;
using System;

namespace VatFilingPricingTool.Api.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring dependency injection in the VAT Filing Pricing Tool API.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all services required by the VAT Filing Pricing Tool API.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The service collection for method chaining.</returns>
        public static IServiceCollection AddVatFilingServices(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            // Register services by category
            services
                .AddDatabaseServices(configuration)
                .AddAuthenticationServices(configuration)
                .AddCachingServices(configuration)
                .AddBusinessServices()
                .AddInfrastructureServices(configuration);

            return services;
        }

        /// <summary>
        /// Registers database context and repositories.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The service collection for method chaining.</returns>
        private static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext with SQL Server provider
            services.AddDbContext<VatFilingDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }));

            // Register DbContext interface
            services.AddScoped<IVatFilingDbContext>(provider => provider.GetRequiredService<VatFilingDbContext>());

            // Register unit of work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register generic repository
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Register specific repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<ICalculationRepository, CalculationRepository>();
            services.AddScoped<IRuleRepository, RuleRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IIntegrationRepository, IntegrationRepository>();
            services.AddScoped<IAdditionalServiceRepository, AdditionalServiceRepository>();

            return services;
        }

        /// <summary>
        /// Registers authentication and authorization services.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The service collection for method chaining.</returns>
        private static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure authentication options
            var authOptions = new AuthenticationOptions();
            configuration.GetSection("Authentication").Bind(authOptions);
            services.Configure<AuthenticationOptions>(configuration.GetSection("Authentication"));

            // Register token handlers and authentication services
            services.AddSingleton<IJwtTokenHandler, JwtTokenHandler>();
            services.AddScoped<IAzureAdAuthenticationHandler, AzureAdAuthenticationHandler>();

            // Configure JWT bearer authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = authOptions.Issuer,
                    ValidAudience = authOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.SecretKey)),
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });

            // Configure authorization policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrator"));
                options.AddPolicy("PricingAdmin", policy => policy.RequireRole("Administrator", "PricingAdministrator"));
                options.AddPolicy("Accountant", policy => policy.RequireRole("Administrator", "PricingAdministrator", "Accountant"));
                options.AddPolicy("Customer", policy => policy.RequireRole("Administrator", "PricingAdministrator", "Accountant", "Customer"));
                options.AddPolicy("ApiClient", policy => policy.RequireRole("ApiClient"));
            });

            return services;
        }

        /// <summary>
        /// Registers caching services.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The service collection for method chaining.</returns>
        private static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure cache options
            var cacheOptions = new CacheOptions();
            configuration.GetSection("Caching").Bind(cacheOptions);
            services.Configure<CacheOptions>(configuration.GetSection("Caching"));

            // Register Redis cache service as singleton
            services.AddSingleton<ICacheService, RedisCacheService>();

            // Register distributed cache for ASP.NET Core
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cacheOptions.RedisConnectionString;
                options.InstanceName = cacheOptions.InstanceName;
            });

            return services;
        }

        /// <summary>
        /// Registers business logic services.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns>The service collection for method chaining.</returns>
        private static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            // Register core business services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IPricingService, PricingService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IRuleService, RuleService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IIntegrationService, IntegrationService>();
            services.AddScoped<IUserService, UserService>();

            // Register rule engine
            services.AddScoped<IRuleEngine, RuleEngine>();
            
            // Register validation services using FluentValidation
            services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<IPricingService>());

            return services;
        }

        /// <summary>
        /// Registers infrastructure services.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The service collection for method chaining.</returns>
        private static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure storage options
            var storageOptions = new StorageOptions();
            configuration.GetSection("Storage").Bind(storageOptions);
            services.Configure<StorageOptions>(configuration.GetSection("Storage"));

            // Register storage client
            services.AddSingleton<IBlobStorageClient, BlobStorageClient>();

            // Configure ERP integration options
            var erpOptions = new ErpIntegrationOptions();
            configuration.GetSection("ErpIntegration").Bind(erpOptions);
            services.Configure<ErpIntegrationOptions>(configuration.GetSection("ErpIntegration"));

            // Register ERP connectors
            services.AddScoped<IDynamicsConnector, DynamicsConnector>();
            services.AddScoped<IErpConnector, ErpConnector>();

            // Configure OCR processing options
            var ocrOptions = new OcrProcessingOptions();
            configuration.GetSection("OcrProcessing").Bind(ocrOptions);
            services.Configure<OcrProcessingOptions>(configuration.GetSection("OcrProcessing"));

            // Register OCR services
            services.AddScoped<IOcrProcessor, OcrProcessor>();
            services.AddSingleton<IAzureCognitiveServicesClient, AzureCognitiveServicesClient>();

            return services;
        }
    }
}