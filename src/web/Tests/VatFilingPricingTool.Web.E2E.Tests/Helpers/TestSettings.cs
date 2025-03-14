using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace VatFilingPricingTool.Web.E2E.Tests.Helpers
{
    /// <summary>
    /// Static class that provides configuration settings for E2E tests of the VAT Filing Pricing Tool.
    /// </summary>
    public static class TestSettings
    {
        /// <summary>
        /// The base URL of the web application under test.
        /// </summary>
        public static string BaseUrl { get; private set; }

        /// <summary>
        /// The default timeout in milliseconds for test operations.
        /// </summary>
        public static int DefaultTimeout { get; private set; }

        /// <summary>
        /// The type of browser to use for tests (chromium, firefox, or webkit).
        /// </summary>
        public static string BrowserType { get; private set; }

        /// <summary>
        /// Indicates whether to run the browser in headless mode.
        /// </summary>
        public static bool Headless { get; private set; }

        /// <summary>
        /// The path where test screenshots will be saved.
        /// </summary>
        public static string ScreenshotPath { get; private set; }

        /// <summary>
        /// The email address for the standard test user.
        /// </summary>
        public static string TestUserEmail { get; private set; }

        /// <summary>
        /// The password for the standard test user.
        /// </summary>
        public static string TestUserPassword { get; private set; }

        /// <summary>
        /// The email address for the admin test user.
        /// </summary>
        public static string AdminUserEmail { get; private set; }

        /// <summary>
        /// The password for the admin test user.
        /// </summary>
        public static string AdminUserPassword { get; private set; }

        /// <summary>
        /// The email address for the accountant test user.
        /// </summary>
        public static string AccountantUserEmail { get; private set; }

        /// <summary>
        /// The password for the accountant test user.
        /// </summary>
        public static string AccountantUserPassword { get; private set; }

        /// <summary>
        /// The email address for the pricing admin test user.
        /// </summary>
        public static string PricingAdminUserEmail { get; private set; }

        /// <summary>
        /// The password for the pricing admin test user.
        /// </summary>
        public static string PricingAdminUserPassword { get; private set; }

        /// <summary>
        /// The configuration instance used to retrieve test settings.
        /// </summary>
        public static IConfiguration Configuration { get; private set; }

        /// <summary>
        /// Static constructor that initializes the test settings from configuration.
        /// </summary>
        static TestSettings()
        {
            // Build configuration from appsettings.json and environment-specific settings
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", 
                    optional: true, reloadOnChange: true);

            Configuration = builder.Build();

            // Initialize settings with values from configuration or defaults
            BaseUrl = GetConfigurationValue("TestSettings:BaseUrl", "https://localhost:5001");
            DefaultTimeout = GetConfigurationValueInt("TestSettings:DefaultTimeout", 30000);
            BrowserType = GetConfigurationValue("TestSettings:BrowserType", "chromium");
            Headless = GetConfigurationValueBool("TestSettings:Headless", true);
            ScreenshotPath = GetConfigurationValue("TestSettings:ScreenshotPath", "./Screenshots");

            // Test user credentials
            TestUserEmail = GetConfigurationValue("TestSettings:TestUserEmail", "test.user@example.com");
            TestUserPassword = GetConfigurationValue("TestSettings:TestUserPassword", "Password123!");
            
            // Admin user credentials
            AdminUserEmail = GetConfigurationValue("TestSettings:AdminUserEmail", "admin.user@example.com");
            AdminUserPassword = GetConfigurationValue("TestSettings:AdminUserPassword", "Password123!");
            
            // Accountant user credentials
            AccountantUserEmail = GetConfigurationValue("TestSettings:AccountantUserEmail", "accountant.user@example.com");
            AccountantUserPassword = GetConfigurationValue("TestSettings:AccountantUserPassword", "Password123!");
            
            // Pricing admin user credentials
            PricingAdminUserEmail = GetConfigurationValue("TestSettings:PricingAdminUserEmail", "pricing.admin@example.com");
            PricingAdminUserPassword = GetConfigurationValue("TestSettings:PricingAdminUserPassword", "Password123!");

            // Ensure screenshot directory exists
            if (!Directory.Exists(ScreenshotPath))
            {
                Directory.CreateDirectory(ScreenshotPath);
            }
        }

        /// <summary>
        /// Gets a configuration value with a fallback default value.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="defaultValue">The default value to use if the key is not found.</param>
        /// <returns>The configuration value or the default value if not found.</returns>
        public static string GetConfigurationValue(string key, string defaultValue)
        {
            string value = Configuration[key];
            return !string.IsNullOrEmpty(value) ? value : defaultValue;
        }

        /// <summary>
        /// Gets an integer configuration value with a fallback default value.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or not parsable.</param>
        /// <returns>The integer configuration value or the default value if not found or not parsable.</returns>
        public static int GetConfigurationValueInt(string key, int defaultValue)
        {
            string value = GetConfigurationValue(key, defaultValue.ToString());
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        /// <summary>
        /// Gets a boolean configuration value with a fallback default value.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or not parsable.</param>
        /// <returns>The boolean configuration value or the default value if not found or not parsable.</returns>
        public static bool GetConfigurationValueBool(string key, bool defaultValue)
        {
            string value = GetConfigurationValue(key, defaultValue.ToString());
            return bool.TryParse(value, out bool result) ? result : defaultValue;
        }
    }
}