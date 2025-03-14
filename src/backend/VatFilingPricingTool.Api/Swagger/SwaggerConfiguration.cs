using Microsoft.Extensions.DependencyInjection; // Version 6.0.0
using Microsoft.OpenApi.Models; // Version 1.2.3
using Swashbuckle.AspNetCore.Filters; // Version 7.0.2
using Swashbuckle.AspNetCore.SwaggerGen; // Version 6.3.0
using Swashbuckle.AspNetCore.SwaggerUI; // Version 6.3.0
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using VatFilingPricingTool.Api.Swagger.SwaggerExamples;

namespace VatFilingPricingTool.Api.Swagger
{
    /// <summary>
    /// Provides configuration for Swagger/OpenAPI documentation in the VAT Filing Pricing Tool API
    /// </summary>
    public static class SwaggerConfiguration
    {
        /// <summary>
        /// Configures Swagger generation services for the API
        /// </summary>
        /// <param name="services">The service collection to add Swagger services to</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // Register example providers
            services.AddSwaggerExamplesFromAssemblyOf<LoginRequestExample>();

            // Add Swagger generator
            services.AddSwaggerGen(options =>
            {
                // Configure basic Swagger document info
                ConfigureSwaggerDocument(options);
                
                // Configure XML comments
                ConfigureXmlComments(options);
                
                // Configure examples
                ConfigureExamples(options);
            });

            return services;
        }

        /// <summary>
        /// Configures Swagger UI options for the API
        /// </summary>
        /// <param name="options">The Swagger UI options to configure</param>
        public static void ConfigureSwaggerUI(SwaggerUIOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            options.SwaggerEndpoint("/swagger/v1/swagger.json", "VAT Filing Pricing Tool API");
            options.DocumentTitle = "VAT Filing Pricing Tool API";
            
            // Configure OAuth settings for JWT authentication
            options.OAuthClientId("swagger-ui");
            options.OAuthAppName("VAT Filing Pricing Tool API - Swagger UI");
            
            // UI customization
            options.DocExpansion(DocExpansion.List);
            options.EnableDeepLinking();
            options.EnableFilter();
            options.DefaultModelRendering(ModelRendering.Model);
            options.DefaultModelsExpandDepth(2);
            options.DisplayRequestDuration();
            options.EnableValidator();
        }

        /// <summary>
        /// Configures the Swagger document with API information and security definitions
        /// </summary>
        /// <param name="options">The Swagger generation options to configure</param>
        private static void ConfigureSwaggerDocument(SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "VAT Filing Pricing Tool API",
                Version = "v1",
                Description = "API for calculating VAT filing costs across multiple jurisdictions with various service levels and transaction volumes",
                Contact = new OpenApiContact
                {
                    Name = "VAT Filing Pricing Tool Support",
                    Email = "support@vatfilingpricingtool.com",
                    Url = new Uri("https://www.vatfilingpricingtool.com/support")
                },
                License = new OpenApiLicense
                {
                    Name = "Proprietary",
                    Url = new Uri("https://www.vatfilingpricingtool.com/license")
                },
                TermsOfService = new Uri("https://www.vatfilingpricingtool.com/terms")
            });

            // Add JWT Bearer authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
        }

        /// <summary>
        /// Configures XML comments for API documentation
        /// </summary>
        /// <param name="options">The Swagger generation options to configure</param>
        private static void ConfigureXmlComments(SwaggerGenOptions options)
        {
            // Include XML comments from the API assembly
            var assembly = Assembly.GetExecutingAssembly();
            var apiXmlFile = $"{assembly.GetName().Name}.xml";
            var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);
            
            if (File.Exists(apiXmlPath))
            {
                options.IncludeXmlComments(apiXmlPath);
            }
            
            // Include XML comments from the Contracts assembly
            var contractsXmlFile = "VatFilingPricingTool.Contracts.xml";
            var contractsXmlPath = Path.Combine(AppContext.BaseDirectory, contractsXmlFile);
            
            if (File.Exists(contractsXmlPath))
            {
                options.IncludeXmlComments(contractsXmlPath);
            }
            
            // Include XML comments from the Domain assembly
            var domainXmlFile = "VatFilingPricingTool.Domain.xml";
            var domainXmlPath = Path.Combine(AppContext.BaseDirectory, domainXmlFile);
            
            if (File.Exists(domainXmlPath))
            {
                options.IncludeXmlComments(domainXmlPath);
            }
        }

        /// <summary>
        /// Configures example providers for request and response models
        /// </summary>
        /// <param name="options">The Swagger generation options to configure</param>
        private static void ConfigureExamples(SwaggerGenOptions options)
        {
            // Enable Swagger examples
            options.ExampleFilters();
            
            // Add operation filters for security and response headers
            options.OperationFilter<AddResponseHeadersFilter>();
            options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
            
            // Add security requirements for endpoints with [Authorize] attribute
            options.OperationFilter<SecurityRequirementsOperationFilter>();
            
            // Ensure each request/response model has examples
            // These examples are already registered via AddSwaggerExamplesFromAssemblyOf above
            // but we reference them here for clarity and to ensure they're included
            //
            // Authentication
            options.RequestBodyFilter<LoginRequestExample>();
            options.RequestBodyFilter<RegisterRequestExample>();
            
            // Pricing Calculation
            options.RequestBodyFilter<CalculationRequestExample>();
            options.SwaggerResponseFilter<CalculationResponseExample>();
            
            // Reporting
            options.RequestBodyFilter<ReportGenerationRequestExample>();
        }
    }
}