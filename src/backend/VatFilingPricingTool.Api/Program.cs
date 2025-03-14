using Microsoft.AspNetCore.Builder; // Microsoft.AspNetCore.Builder v6.0.0
using Microsoft.AspNetCore.Hosting; // Microsoft.AspNetCore.Hosting v6.0.0
using Microsoft.Extensions.Configuration; // Microsoft.Extensions.Configuration v6.0.0
using Microsoft.Extensions.Configuration.Json; // Microsoft.Extensions.Configuration.Json v6.0.0
using Microsoft.Extensions.Configuration.EnvironmentVariables; // Microsoft.Extensions.Configuration.EnvironmentVariables v6.0.0
using Microsoft.Extensions.Configuration.UserSecrets; // Microsoft.Extensions.Configuration.UserSecrets v6.0.0
using Microsoft.Extensions.DependencyInjection; // Microsoft.Extensions.DependencyInjection v6.0.0
using Microsoft.Extensions.Hosting; // Microsoft.Extensions.Hosting v6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging v6.0.0
using Microsoft.ApplicationInsights.AspNetCore; // Microsoft.ApplicationInsights.AspNetCore v2.20.0
using Serilog; // Serilog v2.10.0
using Serilog.Extensions.Hosting; // Serilog.Extensions.Hosting v4.2.0
using Serilog.Settings.Configuration; // Serilog.Settings.Configuration v3.3.0
using Serilog.Sinks.Console; // Serilog.Sinks.Console v4.0.1
using Serilog.Sinks.ApplicationInsights; // Serilog.Sinks.ApplicationInsights v3.1.0
using System;
using System.Threading.Tasks;
using VatFilingPricingTool.Api.Extensions;
using VatFilingPricingTool.Infrastructure.Logging;
using VatFilingPricingTool.Infrastructure.Configuration;

namespace VatFilingPricingTool.Api
{
    /// <summary>
    /// Contains the main entry point for the VAT Filing Pricing Tool API application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point for the application that configures and runs the web host
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Asynchronous task representing the application execution</returns>
        public static async Task Main(string[] args)
        {
            // Create default host builder with command line arguments
            var host = CreateHostBuilder(args).Build();

            // Access the logger for startup logging
            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Starting web host");
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Host terminated unexpectedly");
            }
            finally
            {
                // Ensure buffered logs are flushed before exit.
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Creates and configures the web host builder with necessary services and configuration
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Configured host builder ready to build the web host</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // Load configuration from appsettings.json
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                    // Load configuration from appsettings.{Environment}.json
                    config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);

                    // Load configuration from environment variables
                    config.AddEnvironmentVariables();

                    // Load configuration from user secrets when in development
                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        try
                        {
                            config.AddUserSecrets<Program>();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error adding user secrets: {ex.Message}");
                        }
                    }
                })
                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                    // Configure Serilog from appsettings.json
                    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}