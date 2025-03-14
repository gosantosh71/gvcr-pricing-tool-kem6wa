using Microsoft.AspNetCore.Builder; // Microsoft.AspNetCore.Builder v6.0.0
using Microsoft.AspNetCore.Hosting; // Microsoft.AspNetCore.Hosting v6.0.0
using Microsoft.AspNetCore.Http; // Microsoft.AspNetCore.Http v6.0.0
using Microsoft.Extensions.Hosting; // Microsoft.Extensions.Hosting v6.0.0
using Swashbuckle.AspNetCore.SwaggerUI; // Swashbuckle.AspNetCore.SwaggerUI v6.3.0
using VatFilingPricingTool.Api.Middleware;
using VatFilingPricingTool.Api.Swagger;

namespace VatFilingPricingTool.Api.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring the ASP.NET Core application request pipeline
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Configures the complete request pipeline for the VAT Filing Pricing Tool API
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="env">The hosting environment</param>
        /// <returns>The application builder for method chaining</returns>
        public static IApplicationBuilder UseVatFilingApiConfiguration(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Validate that app is not null
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            // Validate that env is not null
            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            // Configure exception handling middleware
            app.UseExceptionHandling();

            // Configure HTTPS redirection in non-development environments
            if (!env.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // Configure static files middleware
            app.UseStaticFiles();

            // Configure routing middleware
            app.UseRouting();

            // Configure CORS middleware
            app.UseCustomCors();

            // Configure request logging middleware
            app.UseRequestLogging();

            // Configure authentication middleware
            app.UseCustomAuthentication();

            // Configure authorization middleware
            app.UseAuthorization();

            // Configure Swagger middleware in development environment
            if (env.IsDevelopment())
            {
                app.UseSwaggerWithUI();
            }

            // Configure endpoints middleware
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Return the application builder for method chaining
            return app;
        }

        /// <summary>
        /// Configures Swagger and Swagger UI middleware for API documentation
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>The application builder for method chaining</returns>
        public static IApplicationBuilder UseSwaggerWithUI(this IApplicationBuilder app)
        {
            // Validate that app is not null
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            // Add Swagger middleware to generate OpenAPI specification
            app.UseSwagger();

            // Add SwaggerUI middleware with configuration from SwaggerConfiguration.ConfigureSwaggerUI
            app.UseSwaggerUI(SwaggerConfiguration.ConfigureSwaggerUI);

            // Return the application builder for method chaining
            return app;
        }

        /// <summary>
        /// Configures security headers middleware to enhance API security
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>The application builder for method chaining</returns>
        public static IApplicationBuilder UseSecureHeaders(this IApplicationBuilder app)
        {
            // Validate that app is not null
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            // Add middleware to set security headers on HTTP responses
            app.Use(async (context, next) =>
            {
                // Configure Content-Security-Policy header
                context.Response.Headers.Add("Content-Security-Policy", "default-src 'self';");

                // Configure X-Content-Type-Options header
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

                // Configure X-Frame-Options header
                context.Response.Headers.Add("X-Frame-Options", "DENY");

                // Configure X-XSS-Protection header
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

                // Configure Referrer-Policy header
                context.Response.Headers.Add("Referrer-Policy", "no-referrer");

                await next();
            });

            // Return the application builder for method chaining
            return app;
        }

        /// <summary>
        /// Configures CORS middleware with appropriate policies for the API
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>The application builder for method chaining</returns>
        public static IApplicationBuilder UseCustomCors(this IApplicationBuilder app)
        {
            // Validate that app is not null
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            // Add CORS middleware with the default policy
            app.UseCors();

            // Return the application builder for method chaining
            return app;
        }

        /// <summary>
        /// Configures health check endpoints for monitoring API health
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>The application builder for method chaining</returns>
        public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder app)
        {
            // Validate that app is not null
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            // Map the '/health' endpoint to health check middleware
            app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(
                        System.Text.Json.JsonSerializer.Serialize(new
                        {
                            status = report.Status.ToString(),
                            checks = report.Entries.Select(e => new
                            {
                                name = e.Key,
                                status = e.Value.Status.ToString(),
                                description = e.Value.Description,
                                data = e.Value.Data
                            })
                        })
                    );
                }
            });

            // Return the application builder for method chaining
            return app;
        }
    }
}