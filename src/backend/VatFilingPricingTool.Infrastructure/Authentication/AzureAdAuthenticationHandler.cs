using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt; // System.IdentityModel.Tokens.Jwt v6.15.0
using System.Net.Http; // System.Net.Http v6.0.0
using System.Security.Claims; // System.Security.Claims v6.0.0
using System.Text; // System.Text v6.0.0
using System.Text.Json; // System.Text.Json v6.0.0
using System.Threading.Tasks; // System.Threading.Tasks v6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging v6.0.0
using Microsoft.Extensions.Options; // Microsoft.Extensions.Options v6.0.0
using Microsoft.IdentityModel.Protocols; // Microsoft.IdentityModel.Protocols v6.15.0
using Microsoft.IdentityModel.Protocols.OpenIdConnect; // Microsoft.IdentityModel.Protocols.OpenIdConnect v6.15.0
using Microsoft.IdentityModel.Tokens; // Microsoft.IdentityModel.Tokens v6.15.0
using VatFilingPricingTool.Infrastructure.Authentication;

namespace VatFilingPricingTool.Infrastructure.Authentication
{
    /// <summary>
    /// Interface for Azure AD authentication handling
    /// </summary>
    public interface IAzureAdAuthenticationHandler
    {
        /// <summary>
        /// Validates an Azure AD token
        /// </summary>
        /// <param name="token">The token to validate</param>
        /// <returns>True if the token is valid, otherwise false</returns>
        Task<bool> ValidateTokenAsync(string token);

        /// <summary>
        /// Extracts user information from an Azure AD token
        /// </summary>
        /// <param name="token">The token to extract information from</param>
        /// <returns>Dictionary containing user information extracted from the token</returns>
        Task<Dictionary<string, string>> GetUserInfoFromTokenAsync(string token);
    }

    /// <summary>
    /// Implements Azure Active Directory authentication handling for the VAT Filing Pricing Tool.
    /// This class is responsible for validating Azure AD tokens, extracting user information from tokens,
    /// and facilitating the integration with Microsoft's identity platform.
    /// </summary>
    public class AzureAdAuthenticationHandler : IAzureAdAuthenticationHandler
    {
        private readonly ILogger<AzureAdAuthenticationHandler> _logger;
        private readonly IOptions<AuthenticationOptions> _options;
        private readonly HttpClient _httpClient;
        private OpenIdConnectConfiguration _openIdConfig;
        private TokenValidationParameters _tokenValidationParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureAdAuthenticationHandler"/> class with required dependencies.
        /// </summary>
        /// <param name="logger">The logger for recording authentication events and errors.</param>
        /// <param name="options">Configuration options including Azure AD settings.</param>
        /// <param name="httpClient">HTTP client for communicating with Azure AD endpoints.</param>
        public AzureAdAuthenticationHandler(
            ILogger<AzureAdAuthenticationHandler> logger,
            IOptions<AuthenticationOptions> options,
            HttpClient httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            // Initialize token validation parameters
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidAudience = _options.Value.AzureAd.ClientId
                // Issuer and signing keys will be set when OpenID configuration is loaded
            };

            // Initialize OpenID Connect configuration asynchronously
            // This is not ideal in a constructor, but ensures the handler is ready when needed
            Task.Run(async () => await InitializeOpenIdConfigurationAsync()).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Initializes the OpenID Connect configuration for Azure AD.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task InitializeOpenIdConfigurationAsync()
        {
            try
            {
                _logger.LogInformation("Initializing OpenID Connect configuration for Azure AD");
                
                var azureAdOptions = _options.Value.AzureAd;
                var metadataAddress = $"{azureAdOptions.Instance}{azureAdOptions.TenantId}/.well-known/openid-configuration";
                
                var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    metadataAddress,
                    new OpenIdConnectConfigurationRetriever(),
                    new HttpDocumentRetriever(_httpClient) { RequireHttps = true });

                _openIdConfig = await configManager.GetConfigurationAsync();
                
                // Update token validation parameters with issuer and signing keys
                _tokenValidationParameters.ValidIssuer = _openIdConfig.Issuer;
                _tokenValidationParameters.IssuerSigningKeys = _openIdConfig.SigningKeys;
                
                _logger.LogInformation("Successfully initialized OpenID Connect configuration for Azure AD");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize OpenID Connect configuration for Azure AD");
                throw;
            }
        }

        /// <summary>
        /// Validates an Azure AD token.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        /// <returns>True if the token is valid, otherwise false.</returns>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Token validation failed: Token is null or empty");
                return false;
            }

            _logger.LogInformation("Validating Azure AD token");

            try
            {
                // Ensure OpenID configuration is initialized
                if (_openIdConfig == null)
                {
                    await InitializeOpenIdConfigurationAsync();
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, _tokenValidationParameters, out _);
                
                _logger.LogInformation("Azure AD token validation successful");
                return true;
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning(ex, "Token validation failed: Token has expired");
                return false;
            }
            catch (SecurityTokenValidationException ex)
            {
                _logger.LogWarning(ex, "Token validation failed: Invalid token format or signature");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation failed due to an unexpected error");
                return false;
            }
        }

        /// <summary>
        /// Extracts user information from an Azure AD token.
        /// </summary>
        /// <param name="token">The token to extract information from.</param>
        /// <returns>Dictionary containing user information extracted from the token.</returns>
        public async Task<Dictionary<string, string>> GetUserInfoFromTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("User info extraction failed: Token is null or empty");
                return new Dictionary<string, string>();
            }

            _logger.LogInformation("Extracting user information from Azure AD token");

            try
            {
                // Ensure OpenID configuration is initialized
                if (_openIdConfig == null)
                {
                    await InitializeOpenIdConfigurationAsync();
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out _);
                
                var userInfo = new Dictionary<string, string>();
                
                // Extract standard claims
                userInfo["sub"] = GetClaimValue(principal, "sub");
                userInfo["email"] = GetClaimValue(principal, "email") ?? GetClaimValue(principal, "upn");
                userInfo["given_name"] = GetClaimValue(principal, "given_name");
                userInfo["family_name"] = GetClaimValue(principal, "family_name");
                userInfo["name"] = GetClaimValue(principal, "name");
                userInfo["oid"] = GetClaimValue(principal, "oid"); // Object ID in Azure AD
                
                _logger.LogInformation("Successfully extracted user information from Azure AD token");
                return userInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract user information from Azure AD token");
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Gets a claim value from a claims principal.
        /// </summary>
        /// <param name="principal">The claims principal.</param>
        /// <param name="claimType">The claim type.</param>
        /// <returns>The claim value or null if not found.</returns>
        private string GetClaimValue(ClaimsPrincipal principal, string claimType)
        {
            return principal.FindFirst(claimType)?.Value;
        }
    }
}