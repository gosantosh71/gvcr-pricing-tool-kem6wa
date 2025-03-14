using Microsoft.AspNetCore.Builder; // Microsoft.AspNetCore.Builder v6.0.0
using Microsoft.AspNetCore.Hosting; // Microsoft.AspNetCore.Hosting v6.0.0
using Microsoft.AspNetCore.Mvc; // Microsoft.AspNetCore.Mvc v6.0.0
using Microsoft.Extensions.Configuration; // Microsoft.Extensions.Configuration v6.0.0
using Microsoft.Extensions.DependencyInjection; // Microsoft.Extensions.DependencyInjection v6.0.0
using Microsoft.Extensions.Hosting; // Microsoft.Extensions.Hosting v6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging v6.0.0
using FluentValidation.AspNetCore; // FluentValidation.AspNetCore v10.3.6
using Microsoft.ApplicationInsights.AspNetCore; // Microsoft.ApplicationInsights.AspNetCore v2.20.0
using VatFilingPricingTool.Api.Extensions;
using VatFilingPricingTool.Api.Swagger;
using VatFilingPricingTool.Api.Filters;

namespace VatFilingPricingTool.Api
{
    /// <summary>
    /// Configures the ASP.NET Core application services and request pipeline
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Gets the application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the web hosting environment.
        /// </summary>
        public IWebHostEnvironment Environment { get; }

        /// <summary>
        /// Gets the logger for the Startup class.
        /// </summary>
        private readonly ILogger<Startup> _logger;

        /// <summary>
        /// Initializes a new instance of the Startup class with configuration and environment
        /// </summary>
        /// <param name="configuration">The application configuration</param>
        /// <param name="environment">The web hosting environment</param>
        /// <param name="logger">The logger for the Startup class</param>
        public Startup(IConfiguration configuration, IWebHostEnvironment environment, ILogger<Startup> logger)
        {
            // Validate that configuration is not null
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // Validate that environment is not null
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }

            // Validate that logger is not null
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            // Assign configuration to Configuration property
            Configuration = configuration;

            // Assign environment to Environment property
            Environment = environment;

            // Assign logger to _logger property
            _logger = logger;

            // Log startup initialization
            _logger.LogInformation("Startup initialization started");
        }

        /// <summary>
        /// Configures the application services for dependency injection
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <returns>void</returns>
        public void ConfigureServices(IServiceCollection services)
        {
            // Log service configuration start
            _logger.LogInformation("Configuring application services");

            // Add controllers with options for JSON serialization
            services.AddControllers(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.WriteIndented = Environment.IsDevelopment();
            });

            // Add API versioning with default version 1.0
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            });

            // Add FluentValidation for request validation
            services.AddFluentValidationAutoValidation();

            // Add MVC filters including ApiExceptionFilter and ValidationFilter
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(ApiExceptionFilter));
                options.Filters.Add(typeof(ValidationFilter));
            });

            // Add CORS with policy allowing specified origins
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Add health checks for monitoring application health
            services.AddHealthChecks();

            // Add Swagger documentation using SwaggerConfiguration.AddSwaggerDocumentation
            services.AddSwaggerDocumentation();

            // Add Application Insights for monitoring and telemetry
            services.AddApplicationInsightsTelemetry();

            // Add all VAT Filing services using ServiceCollectionExtensions.AddVatFilingServices
            services.AddVatFilingServices(Configuration);

            // Log service configuration completion
            _logger.LogInformation("Application services configured");
        }

        /// <summary>
        /// Configures the HTTP request pipeline with middleware
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>void</returns>
        public void Configure(IApplicationBuilder app)
        {
            // Log application configuration start
            _logger.LogInformation("Configuring application");

            // Configure the complete request pipeline using ApplicationBuilderExtensions.UseVatFilingApiConfiguration
            app.UseVatFilingApiConfiguration(Environment);

            // Log application configuration completion
            _logger.LogInformation("Application configured");
        }
    }
}