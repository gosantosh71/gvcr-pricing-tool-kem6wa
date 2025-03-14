using System;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity; // Version 1.8.0
using Azure.Security.KeyVault.Secrets; // Version 4.4.0
using Microsoft.Extensions.Configuration; // Version 6.0.0
using Microsoft.Extensions.Logging; // Version 6.0.0
using VatFilingPricingTool.Common.Configuration;

namespace VatFilingPricingTool.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration options for Azure Key Vault connection
    /// </summary>
    public class KeyVaultOptions
    {
        /// <summary>
        /// Gets or sets the URI of the Azure Key Vault
        /// </summary>
        public string VaultUri { get; set; }

        /// <summary>
        /// Gets or sets the Azure AD tenant ID for authentication
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the client ID for authentication
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret for authentication
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use client secret authentication
        /// </summary>
        public bool UseClientSecret { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use managed identity authentication
        /// </summary>
        public bool UseManagedIdentity { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyVaultOptions"/> class with default values
        /// </summary>
        public KeyVaultOptions()
        {
            // Initialize with default values
            VaultUri = string.Empty;
            TenantId = string.Empty;
            ClientId = string.Empty;
            ClientSecret = string.Empty;
            UseClientSecret = false;
            UseManagedIdentity = true;  // Default to using managed identity
        }
    }

    /// <summary>
    /// Provides access to secrets stored in Azure Key Vault
    /// </summary>
    public class AzureKeyVaultConfiguration
    {
        /// <summary>
        /// Gets the configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the logger
        /// </summary>
        public ILogger<AzureKeyVaultConfiguration> Logger { get; }

        /// <summary>
        /// Gets the Key Vault options
        /// </summary>
        public KeyVaultOptions Options { get; }

        /// <summary>
        /// Gets the secret client
        /// </summary>
        public SecretClient SecretClient { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureKeyVaultConfiguration"/> class
        /// </summary>
        /// <param name="configuration">The configuration instance</param>
        /// <param name="logger">The logger instance</param>
        public AzureKeyVaultConfiguration(IConfiguration configuration, ILogger<AzureKeyVaultConfiguration> logger)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Load options from configuration
            Options = ConfigurationHelper.Bind<KeyVaultOptions>(configuration, "KeyVault");

            // Validate options
            if (string.IsNullOrEmpty(Options.VaultUri))
            {
                throw new ArgumentException("Key Vault URI must be specified in configuration", nameof(Options.VaultUri));
            }

            // Create the appropriate credential based on configuration
            TokenCredential credential;
            if (Options.UseManagedIdentity)
            {
                Logger.LogInformation("Using Managed Identity for Key Vault authentication");
                credential = new DefaultAzureCredential();
            }
            else if (Options.UseClientSecret)
            {
                if (string.IsNullOrEmpty(Options.TenantId) || 
                    string.IsNullOrEmpty(Options.ClientId) || 
                    string.IsNullOrEmpty(Options.ClientSecret))
                {
                    throw new ArgumentException("TenantId, ClientId, and ClientSecret must be specified when UseClientSecret is true");
                }

                Logger.LogInformation("Using Client Secret for Key Vault authentication");
                credential = new ClientSecretCredential(Options.TenantId, Options.ClientId, Options.ClientSecret);
            }
            else
            {
                // Default to DefaultAzureCredential which tries multiple authentication methods
                Logger.LogInformation("Using Default Azure Credential for Key Vault authentication");
                credential = new DefaultAzureCredential();
            }

            // Create the secret client
            SecretClient = new SecretClient(new Uri(Options.VaultUri), credential);
        }

        /// <summary>
        /// Retrieves a secret from Azure Key Vault by name
        /// </summary>
        /// <param name="secretName">The name of the secret to retrieve</param>
        /// <returns>The secret value as a string</returns>
        /// <exception cref="ArgumentException">Thrown when secretName is null or empty</exception>
        /// <exception cref="InvalidOperationException">Thrown when the secret cannot be retrieved</exception>
        public async Task<string> GetSecretAsync(string secretName)
        {
            if (string.IsNullOrEmpty(secretName))
            {
                throw new ArgumentException("Secret name cannot be null or empty", nameof(secretName));
            }

            Logger.LogInformation("Attempting to retrieve secret: {SecretName}", secretName);

            try
            {
                KeyVaultSecret secret = await SecretClient.GetSecretAsync(secretName);
                return secret.Value;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                Logger.LogError("Secret not found: {SecretName}", secretName);
                throw new InvalidOperationException($"Secret not found: {secretName}", ex);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to retrieve secret: {SecretName}", secretName);
                throw new InvalidOperationException($"Failed to retrieve secret: {secretName}", ex);
            }
        }

        /// <summary>
        /// Sets a secret in Azure Key Vault
        /// </summary>
        /// <param name="secretName">The name of the secret to set</param>
        /// <param name="secretValue">The value of the secret</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="ArgumentException">Thrown when secretName is null or empty</exception>
        /// <exception cref="ArgumentNullException">Thrown when secretValue is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when the secret cannot be set</exception>
        public async Task SetSecretAsync(string secretName, string secretValue)
        {
            if (string.IsNullOrEmpty(secretName))
            {
                throw new ArgumentException("Secret name cannot be null or empty", nameof(secretName));
            }

            if (secretValue == null)
            {
                throw new ArgumentNullException(nameof(secretValue), "Secret value cannot be null");
            }

            Logger.LogInformation("Setting secret: {SecretName}", secretName);

            try
            {
                await SecretClient.SetSecretAsync(secretName, secretValue);
                Logger.LogInformation("Successfully set secret: {SecretName}", secretName);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to set secret: {SecretName}", secretName);
                throw new InvalidOperationException($"Failed to set secret: {secretName}", ex);
            }
        }

        /// <summary>
        /// Deletes a secret from Azure Key Vault
        /// </summary>
        /// <param name="secretName">The name of the secret to delete</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="ArgumentException">Thrown when secretName is null or empty</exception>
        /// <exception cref="InvalidOperationException">Thrown when the secret cannot be deleted</exception>
        public async Task DeleteSecretAsync(string secretName)
        {
            if (string.IsNullOrEmpty(secretName))
            {
                throw new ArgumentException("Secret name cannot be null or empty", nameof(secretName));
            }

            Logger.LogInformation("Deleting secret: {SecretName}", secretName);

            try
            {
                Azure.Response<DeleteSecretOperation> operation = await SecretClient.StartDeleteSecretAsync(secretName);
                // Wait for completion
                await operation.WaitForCompletionAsync();
                Logger.LogInformation("Successfully deleted secret: {SecretName}", secretName);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to delete secret: {SecretName}", secretName);
                throw new InvalidOperationException($"Failed to delete secret: {secretName}", ex);
            }
        }

        /// <summary>
        /// Checks if a secret exists in Azure Key Vault
        /// </summary>
        /// <param name="secretName">The name of the secret to check</param>
        /// <returns>True if the secret exists, false otherwise</returns>
        /// <exception cref="ArgumentException">Thrown when secretName is null or empty</exception>
        public async Task<bool> SecretExistsAsync(string secretName)
        {
            if (string.IsNullOrEmpty(secretName))
            {
                throw new ArgumentException("Secret name cannot be null or empty", nameof(secretName));
            }

            try
            {
                await SecretClient.GetSecretAsync(secretName);
                return true;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                // Secret doesn't exist
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error checking if secret exists: {SecretName}", secretName);
                throw;
            }
        }
    }
}