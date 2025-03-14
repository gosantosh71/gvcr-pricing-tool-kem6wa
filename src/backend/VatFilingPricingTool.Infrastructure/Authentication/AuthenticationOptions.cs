using System; // System v6.0.0

namespace VatFilingPricingTool.Infrastructure.Authentication
{
    /// <summary>
    /// Configuration options for authentication in the VAT Filing Pricing Tool.
    /// This class provides a strongly-typed configuration container for JWT token settings
    /// and other authentication parameters used throughout the application.
    /// </summary>
    public class AuthenticationOptions
    {
        /// <summary>
        /// Gets or sets the JWT token issuer.
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets the JWT token audience.
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// Gets or sets the secret key used for JWT token signing.
        /// This should be a strong, randomly generated key stored securely.
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// Gets or sets the access token expiration time in minutes.
        /// Default value is 60 minutes (1 hour).
        /// </summary>
        public int TokenExpirationMinutes { get; set; }

        /// <summary>
        /// Gets or sets the refresh token expiration time in days.
        /// Default value is 7 days (1 week).
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the token issuer should be validated.
        /// Default value is true.
        /// </summary>
        public bool ValidateIssuer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the token audience should be validated.
        /// Default value is true.
        /// </summary>
        public bool ValidateAudience { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the token lifetime should be validated.
        /// Default value is true.
        /// </summary>
        public bool ValidateLifetime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the issuer signing key should be validated.
        /// Default value is true.
        /// </summary>
        public bool ValidateIssuerSigningKey { get; set; }

        /// <summary>
        /// Gets or sets the Azure Active Directory integration configuration options.
        /// </summary>
        public AzureAdOptions AzureAd { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationOptions"/> class with default values.
        /// </summary>
        public AuthenticationOptions()
        {
            // Set default values for token validation
            ValidateIssuer = true;
            ValidateAudience = true;
            ValidateLifetime = true;
            ValidateIssuerSigningKey = true;
            
            // Set default token expiration values
            TokenExpirationMinutes = 60; // 1 hour
            RefreshTokenExpirationDays = 7; // 1 week
            
            // Initialize Azure AD options with defaults
            AzureAd = new AzureAdOptions();
        }
    }

    /// <summary>
    /// Configuration options for Azure Active Directory integration.
    /// This class provides settings needed for Azure AD authentication in the VAT Filing Pricing Tool.
    /// </summary>
    public class AzureAdOptions
    {
        /// <summary>
        /// Gets or sets the Azure AD instance URL.
        /// Default value is "https://login.microsoftonline.com/".
        /// </summary>
        public string Instance { get; set; }

        /// <summary>
        /// Gets or sets the Azure AD tenant ID (directory ID).
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the client ID (application ID) registered in Azure AD.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the Azure AD domain name.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the callback path for OpenID Connect authentication.
        /// Default value is "/signin-oidc".
        /// </summary>
        public string CallbackPath { get; set; }

        /// <summary>
        /// Gets or sets the signed out callback path for OpenID Connect.
        /// Default value is "/signout-callback-oidc".
        /// </summary>
        public string SignedOutCallbackPath { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureAdOptions"/> class with default values.
        /// </summary>
        public AzureAdOptions()
        {
            // Set default values for Azure AD integration
            Instance = "https://login.microsoftonline.com/";
            CallbackPath = "/signin-oidc";
            SignedOutCallbackPath = "/signout-callback-oidc";
        }
    }
}