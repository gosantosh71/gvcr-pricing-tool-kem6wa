using Microsoft.Extensions.Configuration; // Version 6.0.0
using Microsoft.Extensions.Configuration.Binder; // Version 6.0.0
using Microsoft.Extensions.Hosting; // Version 6.0.0
using System;
using System.Threading.Tasks;
using VatFilingPricingTool.Common.Configuration;

namespace VatFilingPricingTool.Infrastructure.Configuration
{
    /// <summary>
    /// Manages application configuration settings and provides access to configuration values from various sources
    /// including appsettings.json, environment variables, and Azure Key Vault.
    /// </summary>
    public class AppConfiguration
    {
        /// <summary>
        /// Gets the configuration instance.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the hosting environment information.
        /// </summary>
        public IHostEnvironment Environment { get; }

        /// <summary>
        /// Gets the Azure Key Vault configuration provider, if enabled.
        /// </summary>
        public AzureKeyVaultConfiguration KeyVaultConfiguration { get; }

        /// <summary>
        /// Gets a value indicating whether Azure Key Vault integration is enabled.
        /// </summary>
        public bool UseKeyVault { get; }

        /// <summary>
        /// Initializes a new instance of the AppConfiguration class with the specified configuration and environment.
        /// </summary>
        /// <param name="configuration">The configuration instance to use.</param>
        /// <param name="environment">The hosting environment information.</param>
        /// <param name="keyVaultConfiguration">Optional Key Vault configuration provider.</param>
        /// <exception cref="ArgumentNullException">Thrown when configuration or environment is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when Key Vault is enabled in configuration but no KeyVaultConfiguration is provided.</exception>
        public AppConfiguration(
            IConfiguration configuration, 
            IHostEnvironment environment,
            AzureKeyVaultConfiguration keyVaultConfiguration = null)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Environment = environment ?? throw new ArgumentNullException(nameof(environment));

            // Determine whether to use Key Vault based on configuration
            UseKeyVault = ConfigurationHelper.GetValue<bool>(Configuration, "KeyVault:Enabled", false);
            
            // Validate Key Vault configuration
            if (UseKeyVault && keyVaultConfiguration == null)
            {
                throw new InvalidOperationException(
                    "Azure Key Vault is enabled in the configuration, but no KeyVaultConfiguration was provided");
            }
            
            // Use the provided Key Vault configuration if enabled
            if (UseKeyVault)
            {
                KeyVaultConfiguration = keyVaultConfiguration;
            }
        }

        /// <summary>
        /// Gets a typed configuration value by path with optional default value.
        /// </summary>
        /// <typeparam name="T">The type to convert the value to.</typeparam>
        /// <param name="path">Path to the configuration value.</param>
        /// <param name="defaultValue">Default value to return if not found.</param>
        /// <returns>The configuration value or default if not found.</returns>
        /// <exception cref="ArgumentException">Thrown when path is null or empty.</exception>
        public T GetValue<T>(string path, T defaultValue = default)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));

            return ConfigurationHelper.GetValue<T>(Configuration, path, defaultValue);
        }

        /// <summary>
        /// Gets a required typed configuration value by path, throwing an exception if not found.
        /// </summary>
        /// <typeparam name="T">The type to convert the value to.</typeparam>
        /// <param name="path">Path to the configuration value.</param>
        /// <returns>The configuration value.</returns>
        /// <exception cref="ArgumentException">Thrown when path is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the configuration value is missing.</exception>
        public T GetRequiredValue<T>(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));

            return ConfigurationHelper.GetRequiredValue<T>(Configuration, path);
        }

        /// <summary>
        /// Gets a connection string by name from the configuration.
        /// </summary>
        /// <param name="name">Name of the connection string.</param>
        /// <returns>The connection string value.</returns>
        /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
        public string GetConnectionString(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Connection string name cannot be null or empty", nameof(name));

            return Configuration.GetConnectionString(name);
        }

        /// <summary>
        /// Gets a required connection string by name, throwing an exception if not found.
        /// </summary>
        /// <param name="name">Name of the connection string.</param>
        /// <returns>The connection string value.</returns>
        /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection string is missing.</exception>
        public string GetRequiredConnectionString(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Connection string name cannot be null or empty", nameof(name));

            string connectionString = Configuration.GetConnectionString(name);

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Required connection string '{name}' is missing");

            return connectionString;
        }

        /// <summary>
        /// Gets a secret from Azure Key Vault by name.
        /// </summary>
        /// <param name="secretName">The name of the secret to retrieve.</param>
        /// <returns>The secret value.</returns>
        /// <exception cref="ArgumentException">Thrown when secretName is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when Key Vault is not configured or enabled.</exception>
        public async Task<string> GetSecretAsync(string secretName)
        {
            if (string.IsNullOrEmpty(secretName))
                throw new ArgumentException("Secret name cannot be null or empty", nameof(secretName));

            if (!UseKeyVault)
                throw new InvalidOperationException("Azure Key Vault integration is not enabled in the configuration");

            if (KeyVaultConfiguration == null)
                throw new InvalidOperationException("Azure Key Vault is not configured");

            return await KeyVaultConfiguration.GetSecretAsync(secretName);
        }

        /// <summary>
        /// Binds a configuration section to a new instance of a specified type.
        /// </summary>
        /// <typeparam name="T">The type to bind to.</typeparam>
        /// <param name="sectionPath">Path to the configuration section.</param>
        /// <returns>A new instance of T with properties set from the configuration.</returns>
        /// <exception cref="ArgumentException">Thrown when sectionPath is null or empty.</exception>
        public T Bind<T>(string sectionPath) where T : new()
        {
            if (string.IsNullOrEmpty(sectionPath))
                throw new ArgumentException("Section path cannot be null or empty", nameof(sectionPath));

            return ConfigurationHelper.Bind<T>(Configuration, sectionPath);
        }

        /// <summary>
        /// Binds a configuration section to an existing instance of a specified type.
        /// </summary>
        /// <typeparam name="T">The type to bind to.</typeparam>
        /// <param name="sectionPath">Path to the configuration section.</param>
        /// <param name="existingInstance">The existing instance to bind to.</param>
        /// <exception cref="ArgumentException">Thrown when sectionPath is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when existingInstance is null.</exception>
        public void BindExisting<T>(string sectionPath, T existingInstance)
        {
            if (string.IsNullOrEmpty(sectionPath))
                throw new ArgumentException("Section path cannot be null or empty", nameof(sectionPath));

            if (existingInstance == null)
                throw new ArgumentNullException(nameof(existingInstance), "Existing instance cannot be null");

            Configuration.GetSection(sectionPath).Bind(existingInstance);
        }

        /// <summary>
        /// Gets a configuration section by path.
        /// </summary>
        /// <param name="sectionPath">Path to the configuration section.</param>
        /// <returns>The configuration section at the specified path.</returns>
        /// <exception cref="ArgumentException">Thrown when sectionPath is null or empty.</exception>
        public IConfigurationSection GetSection(string sectionPath)
        {
            if (string.IsNullOrEmpty(sectionPath))
                throw new ArgumentException("Section path cannot be null or empty", nameof(sectionPath));

            return Configuration.GetSection(sectionPath);
        }

        /// <summary>
        /// Checks if the current environment matches the specified environment name.
        /// </summary>
        /// <param name="environmentName">The environment name to check.</param>
        /// <returns>True if the current environment matches the specified name.</returns>
        /// <exception cref="ArgumentException">Thrown when environmentName is null or empty.</exception>
        public bool IsEnvironment(string environmentName)
        {
            if (string.IsNullOrEmpty(environmentName))
                throw new ArgumentException("Environment name cannot be null or empty", nameof(environmentName));

            return Environment.IsEnvironment(environmentName);
        }

        /// <summary>
        /// Checks if the current environment is Development.
        /// </summary>
        /// <returns>True if the current environment is Development.</returns>
        public bool IsDevelopment()
        {
            return Environment.IsDevelopment();
        }

        /// <summary>
        /// Checks if the current environment is Production.
        /// </summary>
        /// <returns>True if the current environment is Production.</returns>
        public bool IsProduction()
        {
            return Environment.IsProduction();
        }

        /// <summary>
        /// Checks if the current environment is Staging.
        /// </summary>
        /// <returns>True if the current environment is Staging.</returns>
        public bool IsStaging()
        {
            return Environment.IsStaging();
        }
    }
}